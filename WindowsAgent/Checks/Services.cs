using System;
using System.Collections.Generic;
using System.Management;
using System.ServiceProcess;

namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Services : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "services";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            int index = 0;
            var data = new CheckResult();
            ServiceController[] services = ServiceController.GetServices();
            Item[] items = new Item[services.Length];

            foreach (var service in services)
            {
                var mp = new ManagementPath(string.Format("Win32_Service.Name='{0}'", service.ServiceName));
                var mo = new ManagementObject(mp);

                items[index] = new Item
                {
                    ["name"] = service.ServiceName,
                    ["DesktopInteract"] = Convert.ToBoolean(mo["DesktopInteract"]),
                    ["DisplayName"] = Convert.ToString(mo["DisplayName"]),
                    ["ExitCode"] = Convert.ToUInt32(mo["ExitCode"]),
                    ["PathName"] = Convert.ToString(mo["PathName"]),
                    ["ServiceSpecificExitCode"] = Convert.ToUInt32(mo["ServiceSpecificExitCode"]),
                    ["ServiceType"] = Convert.ToString(mo["ServiceType"]),
                    ["Started"] = Convert.ToBoolean(mo["Started"]),
                    ["StartMode"] = Convert.ToString(mo["StartMode"]),
                    ["StartName"] = Convert.ToString(mo["StartName"]),
                    ["State"] = Convert.ToString(mo["State"]),
                    ["Status"] = Convert.ToString(mo["Status"]),
                };
                index++;
            }

            data.AddType("services", items);
            return data;
        }
    }
}
