using System.Collections.Generic;
using System.Diagnostics;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;

    internal class Memory : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "memory";  // Check key.        

        private readonly Dictionary<string, string> _counters = new Dictionary<string, string>{
            {"AvailableKBytes", "Available KBytes"},
            {"PercentCommitedBytesInUse", "% Committed Bytes In Use"},
        };
        private readonly Cache _counterCache = new Cache();

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();

            Counters.GetSingle("Memory", _counterCache);
            Item[] items = Counters.ToItemList(_counters, _counterCache);

            data.AddType("memory", items);

            return data;
        }
    }
}