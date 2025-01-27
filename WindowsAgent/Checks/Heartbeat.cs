using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    internal class Heartbeat : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "heartbeat";  // Check key.
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            Item[] items = new Item[1];
            items[0] = new Item
            {
                ["name"] = "heartbeat",
                ["time"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            data.AddType("heartbeat", items);
            return data;
        }
    }
}
