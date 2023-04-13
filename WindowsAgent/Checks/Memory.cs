using System;
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

        private readonly string _counterCategrory = "Memory";
        private readonly string[] _counterNames = {
            "Available KBytes",
            "% Committed Bytes In Use",
        };
        private readonly Cache _counterCache = new Cache();
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            int index = 0;
            Counters.GetSingle(_counterCategrory, _counterNames, _counterCache);
            Item[] items = new Item[_counterCache.Count];

            foreach (var instance in _counterCache)
            {
                var item = new Item
                {
                    ["name"] = instance.Key,
                    ["AvailableKBytes"] = Convert.ToInt32(instance.Value["Available KBytes"].NextValue()),
                    ["PercentCommitedBytesInUse"] = instance.Value["% Committed Bytes In Use"].NextValue(),
                };
                items[index++] = item;
            }

            data.AddType("memory", items);

            return data;
        }
    }
}