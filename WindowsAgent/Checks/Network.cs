using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.IO;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;

    internal class Network : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "network";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }
        private Dictionary<string, string> counters = new Dictionary<string, string>{
            {"BytesReceivedPersec", "Bytes Received/sec"},
            {"BytesSentPersec", "Bytes Sent/sec"},
            // {"CurrentBandwidth", "Current Bandwidth"},
            // {"PacketsOutboundDiscarded", "Packets Outbound Discarded"},
            // {"PacketsOutboundErrors", "Packets Outbound Errors"},
            // {"PacketsReceivedDiscarded", "Packets Received Discarded"},
            // {"PacketsReceivedErrors", "Packets Received Errors"},
            // {"OutputQueueLength", "Output Queue Length"},
        };

        private Cache counterCache = new Cache();

        public override CheckResult Run()
        {

            //int index = 0;
            //var data = new CheckResult();
            //NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            //Item[] items = new Item[interfaces.Length];
            //foreach (NetworkInterface netinterface in interfaces)
            //{
            //    items[index++] = new Item
            //    {
            //        ["name"] = netinterface.Name.ToLower(),
            //    };
            //}
            var data = new CheckResult();
            Counters.Get("Network Interface", counterCache);
            Item[] items = Counters.ToItemList(counters, counterCache);

            data.AddType("interface", items);
            return data;
        }
    }
}
