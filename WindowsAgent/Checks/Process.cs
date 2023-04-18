using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace WindowsAgent.Checks
{
    using Aggr = Dictionary<string, Dictionary<string, List<float>>>;
    using AggrItem = Dictionary<string, List<float>>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;
    using Item = Dictionary<string, object>;

    internal class Processs : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "process";  // Check key.
        private readonly string _counterCategrory = "Process";
        private readonly string[] _counterNames = {
            "Creating Process ID",
            "Elapsed Time",
            "Handle Count",
            "ID Process",
            "Page Faults/sec",
            "Page File Bytes",
            "Page File Bytes Peak",
            "% Privileged Time",
            "% Processor Time",
            "% User Time",
            "Pool Nonpaged Bytes",
            "Pool Paged Bytes",
            "Priority Base",
            "Private Bytes",
            "Thread Count",
            "Virtual Bytes",
            "Virtual Bytes Peak",
            "Working Set",
            "Working Set Peak",
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

            Aggr aggr = new Aggr();
            foreach (var instance in _counterCache)
            {
                if (instance.Key != "_Total")
                {
                    var counterValues = new Dictionary<string, float>();
                    var hasError = false;
                    foreach (var counter in instance.Value)
                    {
                        try
                        {
                            counterValues[counter.Key] = counter.Value.NextValue();
                        }
                        catch
                        {
                            if (Config.IsDebug())
                            {
                                string e = string.Format("Failed to retrieve counter value ({0}-{1})", instance.Key, counter.Key);
                                Logger.Write(e, EventLogEntryType.Warning, EventId.InitRegistry);
                            }
                            hasError = true;
                            break;
                        }
                    }

                    if (!hasError)
                    {
                        string name = instance.Key.Split('#')[0];
                        if (!aggr.ContainsKey(name))
                        {
                            aggr[name] = new AggrItem();
                            foreach (var counter in instance.Value)
                            {
                                aggr[name][counter.Key] = new List<float>();
                            }
                        }

                        foreach (var counterValue in counterValues)
                        {
                            aggr[name][counterValue.Key].Add(counterValue.Value);
                        }
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
