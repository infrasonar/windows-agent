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
            CheckResult data = new CheckResult();
            ServiceController[] services = ServiceController.GetServices();

            Item[] items = new Item[services.Length];
            int index = 0;

            foreach (ServiceController service in services)
            {
                ManagementPath mp = new ManagementPath(string.Format("Win32_Service.Name='{0}'", service.ServiceName));
                ManagementObject mo = new ManagementObject(mp);

                items[index] = new Item
                {
                    ["name"] = service.ServiceName,
                    ["Description"] = Convert.ToString(mo["Description"]),
                    ["DesktopInteract"] = Convert.ToBoolean(mo["DesktopInteract"]),
                    ["DisplayName"] = Convert.ToString(mo["DisplayName"]),
                    ["ExitCode"] = Convert.ToInt32(mo["ExitCode"]),
                    ["PathName"] = Convert.ToString(mo["PathName"]),
                    ["ServiceSpecificExitCode"] = Convert.ToInt32(mo["ServiceSpecificExitCode"]),
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
