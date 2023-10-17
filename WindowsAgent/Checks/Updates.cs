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

            Item[] updatesItems = new Item[updates.Count];
            Item[] lastItems = new Item[1];
            int lastIndex = -1;
            long last = 0;

            foreach (ManagementObject mo in updates)
            {
                long installedOn = (long)DateTime.ParseExact((string)mo["InstalledOn"], "M/d/yyyy", null).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                if (installedOn > last)
                {
                    lastIndex = index;
                    last = installedOn;
                }
                updatesItems[index++] = new Item
                {
                    ["name"] = mo["HotFixID"],
                    ["Description"] = mo["Description"],
                    ["FixComments"] = mo["FixComments"],
                    ["InstalledOn"] = installedOn,
                    ["ServicePackInEffect"] = mo["ServicePackInEffect"],
                };
            }

            if (lastIndex >= 0)
            {
                lastItems[0] = updatesItems[lastIndex];
                data.AddType("last", lastItems);
            }

            data.AddType("updates", updatesItems);
            return data;
        }
    }
}
