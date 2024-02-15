using System;
using System.Collections.Generic;
using System.Management;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

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
            Item[] memoryItems = new Item[1];
            List<Item> pageFileItems = new List<Item>();
            PerfomanceInfoData pdata = PsApiWrapper.GetPerformanceInfo();
            decimal percentUsed = 100 - pdata.PhysicalAvailableBytes / (decimal)pdata.PhysicalTotalBytes * 100;
            decimal commitPercentUsed = pdata.CommitTotalPages / (decimal)pdata.CommitLimitPages * 100;

            using (var query = new ManagementObjectSearcher("SELECT Name, AllocatedBaseSize, CurrentUsage FROM Win32_PageFileUsage"))
            {
                foreach (ManagementBaseObject mp in query.Get())
                {
                    string name = Convert.ToString(mp.GetPropertyValue("Name"));
                    long total = Convert.ToUInt32(mp.GetPropertyValue("AllocatedBaseSize")) * 1024 * 1024;
                    long used = Convert.ToUInt32(mp.GetPropertyValue("CurrentUsage")) * 1024 * 1024;
                    long free = total - used;
                    decimal percentage = 0;
                    if (total > 0)
                    {
                        percentage = 100 * used / (decimal)total;
                    }

                    pageFileItems.Add(new Item
                    {
                        ["name"] = name,
                        ["BytesTotal"] = total,
                        ["BytesFree"] = free,
                        ["BytesUsed"] = used,
                        ["PercentUsed"] = percentage,
                    });
                }
            }

            memoryItems[0] = new Item
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
                ["CommitPercentused"] = commitPercentUsed,
            };

            data.AddType("memory", memoryItems);
            data.AddType("pageFile", pageFileItems.ToArray());

            return data;
        }
    }
}