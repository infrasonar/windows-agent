using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;
    using Aggr = Dictionary<string, Dictionary<string, List<float>>>;
    using AggrItem = Dictionary<string, List<float>>;

    internal class Processs : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "process";  // Check key.

        private readonly Dictionary<string, string> _counters = new Dictionary<string, string>{
            {"CreatingProcessID", "Creating Process ID"},
            {"ElapsedTime", "Elapsed Time"},
            {"HandleCount", "Handle Count"},
            {"IDProcess", "ID Process"},
            {"PageFaultsPersec", "Page Faults/sec"},
            {"PageFileBytes", "Page File Bytes"},
            {"PageFileBytesPeak", "Page File Bytes Peak"},
            {"PercentPrivilegedTime", "% Privileged Time"},
            {"PercentProcessorTime", "% Processor Time"},
            {"PercentUserTime", "% User Time"},
            {"PoolNonpagedBytes", "Pool Nonpaged Bytes"},
            {"PoolPagedBytes", "Pool Paged Bytes"},
            {"PriorityBase", "Priority Base"},
            {"PrivateBytes", "Private Bytes"},
            {"ThreadCount", "Thread Count"},
            {"VirtualBytes", "Virtual Bytes"},
            {"VirtualBytesPeak", "Virtual Bytes Peak"},
            {"WorkingSet", "Working Set"},
            {"WorkingSetPeak", "Working Set Peak"},
        }; 
        private readonly Cache _counterCache = new Cache();

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            var index = 0;
            Counters.Get("Process", _counterCache);

            Aggr aggr = new Aggr();
            foreach (var instance in _counterCache)
            {
                if (instance.Key != "_Total")
                {
                    string name = instance.Key.Split('#')[0];
                    if (!aggr.ContainsKey(name))
                    {
                        aggr[name] = new AggrItem();
                        foreach (var counter in _counters)
                        {
                            aggr[name][counter.Value] = new List<float>();
                        }
                    }

                    foreach (var counter in _counters)
                    {
                        aggr[name][counter.Value].Add(instance.Value[counter.Value].NextValue());
                    }
                }
            }

            Item[] items = new Item[aggr.Count];
            foreach (KeyValuePair<string, AggrItem> instance in aggr)
            {
                var item = new Item
                {
                    ["name"] = instance.Key,
                    ["CreatingProcessID"] = instance.Value["Creating Process ID"].ConvertAll(a => (int)a),
                    ["ElapsedTime"] = (int)instance.Value["Elapsed Time"].Sum(),
                    ["HandleCount"] = (int)instance.Value["Handle Count"].Sum(),
                    ["IDProcess"] = instance.Value["ID Process"].ConvertAll(a => (int)a),
                    ["PageFaultsPersec"] = instance.Value["Page Faults/sec"].Sum(),
                    ["PageFileBytes"] = (int)instance.Value["Page File Bytes"].Sum(),
                    ["PageFileBytesPeak"] = (int)instance.Value["Page File Bytes Peak"].Max(),
                    ["PercentPrivilegedTime"] = instance.Value["% Privileged Time"].Sum(),
                    ["PercentProcessorTime"] = instance.Value["% Processor Time"].Sum(),
                    ["PercentUserTime"] = instance.Value["% User Time"].Sum(),
                    ["PoolNonpagedBytes"] = (int)instance.Value["Pool Nonpaged Bytes"].Sum(),
                    ["PoolPagedBytes"] = (int)instance.Value["Pool Paged Bytes"].Sum(),
                    ["PriorityBase"] = instance.Value["Priority Base"].ConvertAll(a => (int)a),
                    ["PrivateBytes"] = (int)instance.Value["Private Bytes"].Sum(),
                    ["ProcessCount"] = instance.Value["Creating Process ID"].Count(),
                    ["ThreadCount"] = (int)instance.Value["Thread Count"].Sum(),
                    ["VirtualBytes"] = (int)instance.Value["Virtual Bytes"].Sum(),
                    ["VirtualBytesPeak"] = (int)instance.Value["Virtual Bytes Peak"].Max(),
                    ["WorkingSet"] = (int)instance.Value["Working Set"].Sum(),
                    ["WorkingSetPeak"] = (int)instance.Value["Working Set Peak"].Max(),
                };
                items[index++] = item;
            };

            data.AddType("process", items);
            return data;
        }
    }
}
