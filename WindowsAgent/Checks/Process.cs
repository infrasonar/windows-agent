using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.IO;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Processs : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "process";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        private Dictionary<string, string> counters = new Dictionary<string, string>{
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

        private Dictionary<string, Dictionary<string, PerformanceCounter>> counterCache = new Dictionary<string, Dictionary<string, PerformanceCounter>>();

        public override CheckResult Run()
        {
            var data = new CheckResult();
            Counters.Get("Process", counterCache);
            Item[] items = Counters.ToItemList(counters, counterCache);

            data.AddType("process", items);
            return data;
        }
    }
}
