using System.Collections.Generic;
using System.Diagnostics;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;

    internal class Processor : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "processor";  // Check key.
        private readonly string _counterCategrory = "Processor";
        private readonly string[] _counterNames = {
            "% Processor Time",
        };
        private readonly Cache _counterCache = new Cache();

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            var index = 0;

            Counters.Get(_counterCategrory, _counterNames, _counterCache);
            Item[] items = new Item[_counterCache.Count - 1];
            Item[] itemsTotal = new Item[1];

            foreach (var instance in _counterCache)
            {
                if (instance.Key != "_Total")
                {
                    items[index++] = new Item
                    {
                        ["name"] = instance.Key,
                        ["PercentProcessorTime"] = instance.Value["% Processor Time"].NextValue(),
                    };
                }
                else
                {
                    itemsTotal[0] = new Item
                    {
                        ["name"] = "total",
                        ["PercentProcessorTime"] = instance.Value["% Processor Time"].NextValue(),
                    };
                }
            }

            data.AddType("processor", items);
            data.AddType("processorTotal", itemsTotal);

            return data;
        }
    }
}