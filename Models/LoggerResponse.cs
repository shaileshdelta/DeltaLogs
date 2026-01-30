using System.Collections.Generic;

namespace DeltaLogs.Models
{
    public class LoggerResponse
    {
        public int? Status { get; set; }
        public string? Message { get; set; }
        public string? Totalrecord { get; set; } = "0";
        public string? TotalLineRecord { get; set; } = "0";
        public dynamic? Data { get; set; }
        public List<string>? Logs { get; set; }
    }
}
