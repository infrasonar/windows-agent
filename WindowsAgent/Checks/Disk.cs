using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WindowsAgent.Checks
{
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;
    using Item = Dictionary<string, object>;

    internal class Disk : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "disk";  // Check key.
        private readonly string _counterCategrory = "LogicalDisk";
        private readonly string _counterCategroryPhysical = "PhysicalDisk";
        private readonly string[] _counterNames = {
            "Disk Reads/sec",
            "Disk Writes/sec",
        };
        private readonly string[] _counterNamesPhysical = {
            "Avg. Disk Read Queue Length",
            "Avg. Disk Write Queue Length",
            "Disk Reads/sec",
            "Disk Read Bytes/sec",
            "Disk Writes/sec",
            "Disk Write Bytes/sec",
            "% Disk Read Time",
            "% Disk Write Time",
        };
        private readonly Cache _counterCache = new Cache();
        private readonly Cache _counterCachePhysical = new Cache();
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }
        public override CheckResult Run()
        {
            var data = new CheckResult();

            Counters.Get(_counterCategrory, _counterNames, _counterCache);
            List<Item> items = new List<Item>();

            foreach (var instance in _counterCache)
            {
                if (instance.Key != "_Total")
                {
                    items.Add(new Item
                    {
                        ["name"] = instance.Key,
                        ["DiskReadsPersec"] = instance.Value["Disk Reads/sec"].NextValue(),
                        ["DiskWritesPersec"] = instance.Value["Disk Writes/sec"].NextValue(),
                    });
                }
            }
            data.AddType("logical", items.ToArray());

            Counters.Get(_counterCategroryPhysical, _counterNamesPhysical, _counterCachePhysical);
            List<Item> itemsPhysical = new List<Item>();

            foreach (var instance in _counterCachePhysical)
            {
                if (instance.Key != "_Total")
                {
                    itemsPhysical.Add(new Item
                    {
                        ["name"] = instance.Key,
                        ["AvgDiskReadQueueLength"] = instance.Value["Avg. Disk Read Queue Length"].NextValue(),
                        ["AvgDiskWriteQueueLength"] = instance.Value["Avg. Disk Write Queue Length"].NextValue(),
                        ["DiskReadBytesPersec"] = instance.Value["Disk Read Bytes/sec"].NextValue(),
                        ["DiskReadsPersec"] = instance.Value["Disk Reads/sec"].NextValue(),
                        ["DiskWriteBytesPersec"] = instance.Value["Disk Write Bytes/sec"].NextValue(),
                        ["DiskWritesPersec"] = instance.Value["Disk Writes/sec"].NextValue(),
                        ["PercentDiskReadTime"] = instance.Value["% Disk Read Time"].NextValue(),
                        ["PercentDiskWriteTime"] = instance.Value["% Disk Write Time"].NextValue(),
                    });
                }
            }

            data.AddType("physical", itemsPhysical.ToArray());
            return data;
        }
    }
}