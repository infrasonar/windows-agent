using System.Collections.Generic;
using System.Diagnostics;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;

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
            Counters.Get("Process", _counterCache);
            Item[] items = Counters.ToItemList(_counters, _counterCache);

            data.AddType("process", items);
            return data;
        }
    }
}
