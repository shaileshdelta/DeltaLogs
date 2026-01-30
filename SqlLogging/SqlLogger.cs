using System.Collections.Generic;
using System.Threading;

namespace DeltaLogs.SqlLogging
{
    public static class SqlLogger
    {
        private static readonly AsyncLocal<List<string>?> _entries = new AsyncLocal<List<string>?>();
        private static readonly AsyncLocal<List<string>?> _failed = new AsyncLocal<List<string>?>();

        public static void BeginRequest()
        {
            _entries.Value = new List<string>();
            _failed.Value = new List<string>();
        }

        public static void Add(string sql)
        {
            if (_entries.Value == null) _entries.Value = new List<string>();
            _entries.Value.Add(sql);
        }

        public static void AddFailed(string sql)
        {
            if (_failed.Value == null) _failed.Value = new List<string>();
            _failed.Value.Add(sql);
        }

        public static List<string>? GetEntries() => _entries.Value;
        public static List<string>? GetFailedEntries() => _failed.Value;

        public static void EndRequest()
        {
            _entries.Value = null;
            _failed.Value = null;
        }
    }
}
