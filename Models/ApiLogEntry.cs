using System;
using System.Collections.Generic;

namespace DeltaLogs.Models
{
    public class ApiLogEntry
    {
        public string? LogId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Method { get; set; }
        public string? Url { get; set; }
        public string? QueryString { get; set; }
        public string? Body { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? ClientIP { get; set; }
        public string? ErrorComponent { get; set; }
        public string? ErrorClass { get; set; }
        public string? ErrorMethod { get; set; }
        public int? ErrorLine { get; set; }
        public string? StackTrace { get; set; }
        public List<string>? FailedSqlEntries { get; set; }
    }
}
