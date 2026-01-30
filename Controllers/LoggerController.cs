using DeltaLogs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DeltaLogs.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoggerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoggerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetLogsByDate(string? date)
        {
            try
            {
                var basePath = _configuration["General:BasePath"];
                if (string.IsNullOrEmpty(basePath))
                {
                    basePath = "wwwroot";
                }
                var relativePath = _configuration["LoggerPath:RelativePath"];
                if (string.IsNullOrEmpty(relativePath))
                {
                    relativePath = "Logs/";
                }
                var relPath = relativePath.Replace("/", "\\");
                var dir = Path.Combine(Directory.GetCurrentDirectory(), basePath, relPath);

                var dt = string.IsNullOrEmpty(date) ? DateTime.UtcNow : DateTime.Parse(date);
                var yearDir = Path.Combine(dir, dt.ToString("yyyy"));
                var monthDir = Path.Combine(yearDir, dt.ToString("MM"));
                var file = Path.Combine(monthDir, dt.ToString("yyyy-MM-dd") + ".log");

                if (!System.IO.File.Exists(file))
                {
                    return Ok(new LoggerResponse
                    {
                        Status = 200,
                        Message = "Data Not Found.",
                        Totalrecord = "0",
                        Data = new List<ApiLogEntry>()
                    });
                }

                var lines = System.IO.File.ReadAllLines(file);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var logs = new List<ApiLogEntry>();
                foreach (var line in lines)
                {
                    try
                    {
                        var entry = JsonSerializer.Deserialize<ApiLogEntry>(line, opts);
                        if (entry != null)
                        {
                            logs.Add(entry);
                        }
                    }
                    catch
                    {
                    }
                }

                var resultData = logs.Select(l => new
                {
                    LogId = l.LogId,
                    Timestamp = l.Timestamp,
                    Method = l.Method,
                    Url = l.Url,
                    DurationMs = l.DurationMs,
                    RequestParams = string.IsNullOrEmpty(l.Body) ? l.QueryString : l.Body,
                    SuccessMessage = l.Success ? "success" : null,
                    Error = l.Success ? null : l.Error,
                    ErrorComponent = l.Success ? null : l.ErrorComponent,
                    ErrorClass = l.Success ? null : l.ErrorClass,
                    ErrorMethod = l.Success ? null : l.ErrorMethod,
                    ErrorLine = l.Success ? null : l.ErrorLine,
                    StackTrace = l.Success ? null : l.StackTrace,
                    FailedSqlEntries = l.FailedSqlEntries,
                    SqlEntries = l.SqlEntries,
                    EndpointName = l.EndpointName,
                    Route = l.Route,
                    CallStack = l.CallStack,
                    StatusCode = l.StatusCode,
                    ClientIP = l.ClientIP
                }).ToList();

                if (resultData.Count == 0)
                {
                    return Ok(new LoggerResponse
                    {
                        Status = 200,
                        Message = "Data Not Found.",
                        Totalrecord = "0",
                        Data = resultData
                    });
                }
                else
                {
                    return Ok(new LoggerResponse
                    {
                        Status = 1,
                        Message = "Logs fetched successfully.",
                        Totalrecord = resultData.Count.ToString(),
                        Data = resultData
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(404, new LoggerResponse
                {
                    Status = 404,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
