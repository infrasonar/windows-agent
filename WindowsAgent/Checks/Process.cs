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

        public override CheckResult Run()
        {
            int index = 0;
            var data = new CheckResult();
            Process[] processes = Process.GetProcesses();
            Item[] items = new Item[processes.Length];
            foreach (Process process in processes)
            {
                items[index++] = new Item
                {
                    ["name"] = process.ProcessName,
                };
            }

            // PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");
            // String[] instances = cat.GetInstanceNames();
            // Item[] items = new Item[instances.Length];

            // foreach (string instancename in instances)
            // {
            //     var counter = new PerformanceCounter("Process", "PercentProccesorTime", instancename);
            //     items[index++] = new Item
            //     {
            //         ["name"] = instancename,
            //         ["PercentProccesorTime"] = counter.NextValue()

            //     };
            // }

            data.AddType("process", items);
            return data;
        }
    }
}
