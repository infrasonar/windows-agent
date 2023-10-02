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
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            Item[] items = new Item[1];
            PerfomanceInfoData pdata = PsApiWrapper.GetPerformanceInfo();            
            decimal percentUsed = 100 - pdata.PhysicalAvailableBytes / (decimal)pdata.PhysicalTotalBytes * 100;

            items[0] = new Item
            {
                ["name"] = "memory",
                ["PhysicalAvailableBytes"] = pdata.PhysicalAvailableBytes,
                ["PhysicalTotalBytes"] = pdata.PhysicalTotalBytes,
                ["CommitTotalPages"] = pdata.CommitTotalPages,
                ["CommitLimitPages"] = pdata.CommitLimitPages,
                ["CommitPeakPages"] = pdata.CommitPeakPages,
                ["SystemCacheBytes"] = pdata.SystemCacheBytes,
                ["KernelTotalBytes"] = pdata.KernelTotalBytes,
                ["KernelPagedBytes"] = pdata.KernelPagedBytes,
                ["KernelNonPagedBytes"] = pdata.KernelNonPagedBytes,
                ["PageSizeBytes"] = pdata.PageSizeBytes,
                ["PercentUsed"] = percentUsed,
            };

            data.AddType("memory", items);
            return data;
        }
    }
}