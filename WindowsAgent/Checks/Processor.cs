using System;
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
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }
        private Dictionary<string, string> counters = new Dictionary<string, string>{
            {"PercentProcessorTime", "% Processor Time"},
        };
        private Cache counterCache = new Cache();

        public override CheckResult Run()
        {

            var data = new CheckResult();

            Counters.Get("Processor", counterCache);
            Item[] items = Counters.ToItemList(counters, counterCache);
            Item[] itemsTotal = Counters.ToItemListTotal(counters, counterCache);

            data.AddType("processor", items);
            data.AddType("processorTotal", itemsTotal);

            return data;
        }
    }
}