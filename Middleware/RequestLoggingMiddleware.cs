using DeltaLogs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using DeltaLogs.SqlLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeltaLogs.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _logDir;
        private static bool IsFrameworkNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns)) return false;
            return ns.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                || ns.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
                || ns.StartsWith("Serilog", StringComparison.OrdinalIgnoreCase)
                || ns.StartsWith("Newtonsoft", StringComparison.OrdinalIgnoreCase)
                || ns.StartsWith("Swashbuckle", StringComparison.OrdinalIgnoreCase)
                || ns.StartsWith("DeltaLogs", StringComparison.OrdinalIgnoreCase);
        }
        private static string? ClassifyComponent(string? ns, string? typeName)
        {
            var name = (typeName ?? "").ToLowerInvariant();
            var nsl = (ns ?? "").ToLowerInvariant();
            if (name.Contains("controller") || nsl.Contains("controller")) return "Controller";
            if (name.Contains("repository") || name.Contains("repo") || nsl.Contains("repository") || nsl.Contains("repo")) return "Repository";
            if (name.Contains("service") || nsl.Contains("service")) return "Service";
            if (name.Contains("handler") || nsl.Contains("handler")) return "Handler";
            if (name.Contains("manager") || nsl.Contains("manager")) return "Manager";
            if (name.Contains("dal") || nsl.Contains("dal") || nsl.Contains("dataaccess")) return "DAL";
            return "App";
        }

        public RequestLoggingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            var basePath = configuration["General:BasePath"];
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = "wwwroot";
            }
            var relativePath = configuration["LoggerPath:RelativePath"];
            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = "Logs";
            }
            var relPath = relativePath.Replace("/", "\\");
            _logDir = Path.Combine(Directory.GetCurrentDirectory(), basePath, relPath);
            if (!Directory.Exists(_logDir))
            {
                Directory.CreateDirectory(_logDir);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            SqlLogger.BeginRequest();
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                SqlLogger.EndRequest();
                return;
            }
            // Exclude the logger controller itself to avoid infinite logging loops or noise
            if (context.Request.Path.StartsWithSegments("/api/Logger/GetLogsByDate", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                SqlLogger.EndRequest();
                return;
            }

            var sw = Stopwatch.StartNew();
            string? body = null;
            try
            {
                if (context.Request.ContentLength > 0 && context.Request.Body != null)
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
            }
            catch
            {
                body = null;
            }

            string? error = null;
            int statusCode = 200;
            string? errorComponent = null;
            string? errorClass = null;
            string? errorMethod = null;
            int? errorLine = null;
            string? stackTrace = null;
            string? responseText = null;
            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;
            try
            {
                await _next(context);
                statusCode = context.Response.StatusCode;
            }
            catch (System.Exception ex)
            {
                statusCode = 500;
                error = ex.Message;
                var trace = new System.Diagnostics.StackTrace(ex, true);
                var frames = trace.GetFrames();
                if (frames != null)
                {
                    foreach (var f in frames)
                    {
                        var m = f.GetMethod();
                        var dt = m?.DeclaringType;
                        var ns = dt?.Namespace ?? "";
                        if (!IsFrameworkNamespace(ns) && dt != null)
                        {
                            errorClass = dt.FullName;
                            errorMethod = m?.Name;
                            errorLine = f.GetFileLineNumber();
                            errorComponent = ClassifyComponent(ns, dt.Name);
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(errorClass))
                    {
                        foreach (var f in frames)
                        {
                            var m = f.GetMethod();
                            var dt = m?.DeclaringType;
                            var ns = dt?.Namespace ?? "";
                            errorClass = dt?.FullName;
                            errorMethod = m?.Name;
                            var ln = f.GetFileLineNumber();
                            if (ln > 0)
                            {
                                errorLine = ln;
                                errorComponent = ClassifyComponent(ns, dt?.Name);
                                break;
                            }
                        }
                    }
                }
                stackTrace = ex.ToString();
                try
                {
                    var __trace = new System.Diagnostics.StackTrace(ex, true);
                    var __frames = __trace.GetFrames();
                    var __cs = new List<string>();
                    if (__frames != null)
                    {
                        foreach (var f in __frames)
                        {
                            var m = f.GetMethod();
                            var dt = m?.DeclaringType;
                            var li = f.GetFileLineNumber();
                            var ns = dt?.Namespace ?? "";
                            if (dt != null && !IsFrameworkNamespace(ns))
                            {
                                __cs.Add($"{dt.FullName}.{m?.Name}" + (li > 0 ? $" @line {li}" : ""));
                            }
                            if (__cs.Count >= 10) break;
                        }
                    }
                    if (__cs.Count > 0)
                    {
                        context.Items["__DeltaLogs_CallStack"] = __cs;
                    }
                }
                catch
                {
                }
                throw;
            }
            finally
            {
                try
                {
                    memStream.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(memStream, Encoding.UTF8, false, 1024, true);
                    responseText = await reader.ReadToEndAsync();
                    memStream.Seek(0, SeekOrigin.Begin);
                    await memStream.CopyToAsync(originalBody);
                }
                catch
                {
                }
                finally
                {
                    context.Response.Body = originalBody;
                }

                int? appStatus = null;
                string? appMessage = null;
                string? appData = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(responseText))
                    {
                        using var doc = JsonDocument.Parse(responseText);
                        var root = doc.RootElement;
                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("status", out var s))
                            {
                                if (s.ValueKind == JsonValueKind.Number)
                                {
                                    appStatus = s.GetInt32();
                                }
                            }
                            else if (root.TryGetProperty("Status", out var s2) && s2.ValueKind == JsonValueKind.Number)
                            {
                                appStatus = s2.GetInt32();
                            }

                            if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                            {
                                appMessage = m.GetString();
                            }
                            else if (root.TryGetProperty("Message", out var m2) && m2.ValueKind == JsonValueKind.String)
                            {
                                appMessage = m2.GetString();
                            }

                            if (root.TryGetProperty("data", out var d))
                            {
                                appData = d.ToString();
                            }
                            else if (root.TryGetProperty("Data", out var d2))
                            {
                                appData = d2.ToString();
                            }
                        }
                    }
                }
                catch
                {
                }

                var endpoint = context.GetEndpoint();
                var cad = endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>();
                if (cad != null && string.IsNullOrEmpty(errorComponent))
                {
                    errorComponent = "Controller";
                    errorClass = cad.ControllerTypeInfo?.FullName;
                    errorMethod = cad.ActionName;
                }
                string? endpointName = endpoint?.DisplayName;
                string? routeText = null;
                if (endpoint is RouteEndpoint re)
                {
                    routeText = re.RoutePattern?.RawText;
                }

                var appFailure = appStatus.HasValue && (appStatus.Value >= 400 || appStatus.Value == 209);
                var successFlag = (statusCode < 400) && !appFailure;
                if (error == null && appFailure)
                {
                    error = string.IsNullOrWhiteSpace(appData) ? appMessage : appData;
                }

                sw.Stop();
                var log = new ApiLogEntry
                {
                    LogId = System.Guid.NewGuid().ToString(),
                    Timestamp = System.DateTime.UtcNow,
                    Method = context.Request.Method,
                    Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    Body = body,
                    StatusCode = statusCode,
                    DurationMs = sw.ElapsedMilliseconds,
                    Success = successFlag,
                    Error = error,
                    ClientIP = context.Connection.RemoteIpAddress?.ToString(),
                    ErrorComponent = errorComponent,
                    ErrorClass = errorClass,
                    ErrorMethod = errorMethod,
                    ErrorLine = errorLine,
                    StackTrace = stackTrace,
                    FailedSqlEntries = context.Items["FailedSqlEntries"] as List<string>,
                    EndpointName = endpointName,
                    Route = routeText
                };
                var cs = context.Items["__DeltaLogs_CallStack"] as List<string>;
                if (cs != null && cs.Count > 0)
                {
                    log.CallStack = cs;
                }
                var sqls = DeltaLogs.SqlLogging.SqlLogger.GetEntries();
                if (sqls != null && sqls.Count > 0)
                {
                    log.SqlEntries = sqls;
                }
                var failed = DeltaLogs.SqlLogging.SqlLogger.GetFailedEntries();
                if (failed != null && failed.Count > 0)
                {
                    log.FailedSqlEntries = failed;
                }
                DeltaLogs.SqlLogging.SqlLogger.EndRequest();

                var json = JsonSerializer.Serialize(log);
                var now = System.DateTime.UtcNow;
                var yearDir = Path.Combine(_logDir, now.ToString("yyyy"));
                var monthDir = Path.Combine(yearDir, now.ToString("MM"));
                if (!Directory.Exists(yearDir)) Directory.CreateDirectory(yearDir);
                if (!Directory.Exists(monthDir)) Directory.CreateDirectory(monthDir);
                var fileName = now.ToString("yyyy-MM-dd") + ".log";
                var filePath = Path.Combine(monthDir, fileName);
                try
                {
                    File.AppendAllText(filePath, json + System.Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                }
            }
        }
    }
}
