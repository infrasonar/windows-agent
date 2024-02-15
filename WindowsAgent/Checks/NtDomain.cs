using System;
using System.Collections.Generic;
using System.Management;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class NtDomain : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "ntDomain";  // Check key.
        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        private static string GetShareType(long shareType)
        {
            switch (shareType)
            {
                case 0: return "Disk Drive";
                case 1: return "Print Queue";
                case 2: return "Device";
                case 3: return "IPC";
                case 2147483648: return "Disk Drive Admin`";
                case 2147483649: return "Print Queue Admin`";
                case 2147483650: return "Device Admin`";
                case 2147483651: return "IPC Admin`";
                default: return "Unknown";
            }
        }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            List<Item> domainItems = new List<Item>();
            List<Item> shareItems = new List<Item>();
            Dictionary<string, long> sessionCount = new Dictionary<string, long>();

            using (var query = new ManagementObjectSearcher("SELECT DomainName, DnsForestName, DomainControllerName FROM Win32_NTDomain WHERE DomainName IS NOT NULL"))
            {
                foreach (ManagementBaseObject mp in query.Get())
                {
                    domainItems.Add(new Item
                    {
                        ["name"] = Convert.ToString(mp.GetPropertyValue("DomainName")),
                        ["DnsForestName"] = Convert.ToString(mp.GetPropertyValue("DnsForestName")),
                        ["DomainControllerName"] = Convert.ToString(mp.GetPropertyValue("DomainControllerName")).Trim(new char[] { '\\' }),
                    });
                }
            }

            using (var query = new ManagementObjectSearcher("SELECT AllowMaximum, Caption, Description, MaximumAllowed, Name, Path, Status, Type FROM Win32_Share"))
            {
                foreach (ManagementBaseObject mp in query.Get())
                {
                    shareItems.Add(new Item
                    {
                        ["name"] = Convert.ToString(mp.GetPropertyValue("Name")),
                        ["AllowMaximum"] = Convert.ToBoolean(mp.GetPropertyValue("AllowMaximum")),
                        ["Caption"] = Convert.ToString(mp.GetPropertyValue("Caption")),
                        ["Description"] = Convert.ToString(mp.GetPropertyValue("Description")),
                        ["MaximumAllowed"] = Convert.ToUInt32(mp.GetPropertyValue("MaximumAllowed")),
                        ["Path"] = Convert.ToString(mp.GetPropertyValue("Path")),
                        ["Status"] = Convert.ToString(mp.GetPropertyValue("Status")),
                        ["Type"] = GetShareType(Convert.ToUInt32(mp.GetPropertyValue("Type"))),
                    });
                }
            }

            data.AddType("domain", domainItems.ToArray());
            data.AddType("share", shareItems.ToArray());

            return data;
        }
    }
}