using System.Collections.Generic;
using System.Diagnostics;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;

    internal class Network : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "network";  // Check key.        

        private readonly Dictionary<string, string> _counters = new Dictionary<string, string>{
            {"BytesReceivedPersec", "Bytes Received/sec"},
            {"BytesSentPersec", "Bytes Sent/sec"},
            {"CurrentBandwidth", "Current Bandwidth"},
            {"PacketsOutboundDiscarded", "Packets Outbound Discarded"},
            {"PacketsOutboundErrors", "Packets Outbound Errors"},
            {"PacketsReceivedDiscarded", "Packets Received Discarded"},
            {"PacketsReceivedErrors", "Packets Received Errors"},
            {"OutputQueueLength", "Output Queue Length"},
        };

        private readonly Cache _counterCache = new Cache();
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            Counters.Get("Network Interface", _counterCache);
            Item[] items = Counters.ToItemList(_counters, _counterCache);

            data.AddType("interface", items);
            return data;
        }
    }
}
