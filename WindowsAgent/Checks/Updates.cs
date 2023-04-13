using System;
using System.Collections.Generic;
using System.Management;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Updates : Check
    {
        private const int _defaultInterval = 240;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "updates";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            int index = 0;
            var data = new CheckResult();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "Select * from Win32_QuickFixEngineering");
            ManagementObjectCollection updates = searcher.Get();

            Item[] items = new Item[updates.Count];

            foreach (ManagementObject mo in updates)
            {
                items[index++] = new Item
                {
                    ["name"] = mo["HotFixID"],
                    ["Description"] = mo["Description"],
                    ["FixComments"] = mo["FixComments"],
                    ["InstalledOn"] = (int)DateTime.ParseExact((string)mo["InstalledOn"], "M/d/yyyy", null).Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    ["ServicePackInEffect"] = mo["ServicePackInEffect"],
                };
            }

            data.AddType("updates", items);
            return data;
        }
    }
}
