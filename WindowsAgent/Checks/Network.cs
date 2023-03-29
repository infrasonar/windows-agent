using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.IO;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Network : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "network";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            int index = 0;
            var data = new CheckResult();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            Item[] items = new Item[interfaces.Length];
            foreach (NetworkInterface netinterface in interfaces)
            {
                items[index++] = new Item
                {
                    ["name"] = netinterface.Name.ToLower(),
                };
            }

            // PerformanceCounterCategory cat = new PerformanceCounterCategory("Network Interface");
            // String[] instances = cat.GetInstanceNames();
            // Item[] items = new Item[instances.Length];

            // foreach (string instancename in instances)
            // {
            //     var BytesReceivedPersec = new PerformanceCounter("Network Interface", "Bytes Received/sec", instancename);
            //     var BytesSentPersec = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instancename);
            //     var CurrentBandwidth = new PerformanceCounter("Network Interface", "Current Bandwidth", instancename);
            //     var PacketsOutboundDiscarded = new PerformanceCounter("Network Interface", "Packets Outbound Discarded", instancename);
            //     var PacketsOutboundErrors = new PerformanceCounter("Network Interface", "Packets Outbound Errors", instancename);
            //     var PacketsReceivedDiscarded = new PerformanceCounter("Network Interface", "Packets Received Discarded", instancename);
            //     var PacketsReceivedErrors = new PerformanceCounter("Network Interface", "Packets Received Errors", instancename);
            //     var OutputQueueLength = new PerformanceCounter("Network Interface", "Output Queue Length", instancename);
            //     items[index++] = new Item
            //     {
            //         ["name"] = instancename,
            //         ["BytesReceivedPersec"] = BytesReceivedPersec.NextValue(),
            //         ["BytesSentPersec"] = BytesSentPersec.NextValue(),
            //         ["CurrentBandwidth"] = CurrentBandwidth.NextValue(),
            //         ["PacketsOutboundDiscarded"] = PacketsOutboundDiscarded.NextValue(),
            //         ["PacketsOutboundErrors"] = PacketsOutboundErrors.NextValue(),
            //         ["PacketsReceivedDiscarded"] = PacketsReceivedDiscarded.NextValue(),
            //         ["PacketsReceivedErrors"] = PacketsReceivedErrors.NextValue(),
            //         ["OutputQueueLength"] = OutputQueueLength.NextValue(),
            //     };
            // }

            data.AddType("interface", items);
            return data;
        }
    }
}
