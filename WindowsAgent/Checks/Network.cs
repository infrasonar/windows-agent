using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace WindowsAgent.Checks
{
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;
    using Item = Dictionary<string, object>;

    internal class Network : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "network";  // Check key.        
        private readonly string _counterCategrory = "Network Interface";
        private readonly string[] _counterNames = {
            "Bytes Received/sec",
            "Bytes Sent/sec",
            "Current Bandwidth",
            "Packets Outbound Discarded",
            "Packets Outbound Errors",
            "Packets Received Discarded",
            "Packets Received Errors",
            "Output Queue Length",
        };
        private readonly Cache _counterCache = new Cache();
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            int index = 0;
            Counters.Get(_counterCategrory, _counterNames, _counterCache);
            Item[] items = new Item[_counterCache.Count];

            foreach (var instance in _counterCache)
            {
                var item = new Item
                {
                    ["name"] = instance.Key,
                    ["BytesReceivedPersec"] = instance.Value["Bytes Received/sec"].NextValue(),
                    ["BytesSentPersec"] = instance.Value["Bytes Sent/sec"].NextValue(),
                    ["CurrentBandwidth"] = Convert.ToInt32(instance.Value["Current Bandwidth"].NextValue()),
                    ["PacketsOutboundDiscarded"] = Convert.ToInt32(instance.Value["Packets Outbound Discarded"].NextValue()),
                    ["PacketsOutboundErrors"] = Convert.ToInt32(instance.Value["Packets Outbound Errors"].NextValue()),
                    ["PacketsReceivedDiscarded"] = Convert.ToInt32(instance.Value["Packets Received Discarded"].NextValue()),
                    ["PacketsReceivedErrors"] = Convert.ToInt32(instance.Value["Packets Received Errors"].NextValue()),
                    ["OutputQueueLength"] = Convert.ToInt32(instance.Value["Output Queue Length"].NextValue()),
                };
                items[index++] = item;
            }

            data.AddType("interface", items);
            return data;
        }
    }
}
