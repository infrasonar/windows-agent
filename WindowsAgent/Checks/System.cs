using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class System : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "system";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();

            Item[] timeItems = new Item[1];
            Item[] infrasonarItems = new Item[1];

            timeItems[0] = new Item
            {
                ["name"] = "time",
                ["Uptime"] = (int)(Stopwatch.GetTimestamp() / Stopwatch.Frequency),
                ["UniversalTime"] = (int)(DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            };

            infrasonarItems[0] = new Item
            {
                ["name"] = "agent",
                ["version"] = InfraSonarAgent.GetVersion(),
            };

            data.AddType("time", timeItems);
            data.AddType("infrasonar", infrasonarItems);

            return data;
        }
    }
}
