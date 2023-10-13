using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;

    internal class Users : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "users";  // Check key.        
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        private static string GetName(string name)
        {
            var splitted = name.Split('"');
            return splitted[1] + "\\" + splitted[3];
        }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            long total = 0;
            long count = -1;
            Item[] remoteItems = new Item[1];
            Item[] loggedOnTotalItems = new Item[1];
            List<Item> loggedOnItems = new List<Item>();
            Dictionary<string, long> sessionCount = new Dictionary<string, long>();

            using (var query = new ManagementObjectSearcher("SELECT Caption FROM Win32_Process WHERE Caption =\'winlogon.exe\'"))
            {               
                foreach (ManagementBaseObject mp in query.Get())
                {
                    count += 1;
                }
                remoteItems[0] = new Item
                {
                    ["name"] = "remote",
                    ["Count"] = count,
                };
            }

            using (var query = new ManagementObjectSearcher("SELECT Antecedent FROM Win32_LoggedOnUser"))
            {
                foreach (ManagementBaseObject mp in query.Get())
                {
                    string name = GetName(Convert.ToString(mp.GetPropertyValue("Antecedent")));
                    if (!sessionCount.ContainsKey(name)) {
                        sessionCount[name] = 0;
                    }
                    sessionCount[name] += 1;
                    total += 1;
                }
            }
            loggedOnTotalItems[0] = new Item
            {
                ["name"] = "total",
                ["SessionCount"] = total,
            };
            foreach (KeyValuePair<string, long> entry in sessionCount)
            {
                loggedOnItems.Add(new Item
                { 
                    ["name"] = entry.Key,
                    ["SessionCount"] = entry.Value,
                });
            }

            data.AddType("remote", remoteItems);
            data.AddType("loggedOn", loggedOnItems.ToArray());
            data.AddType("loggedOnTotal", loggedOnTotalItems);

            return data;
        }
    }
}