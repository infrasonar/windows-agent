using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace WindowsAgent.Checks
{

    internal class System : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "system";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            CheckResult data = new CheckResult();
            Dictionary<string, object>[] items = new Dictionary<string, object>[1];

            items[0]["name"] = "time";
            items[0]["uptime"] = (int)(Stopwatch.GetTimestamp() / Stopwatch.Frequency);
            items[0]["universalTime"] = (int)(DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            data.AddType("system", items);
            return data;
        }
    }
}
