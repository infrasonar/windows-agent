using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Netstat : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "netstat";  // Check key.

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        private static string GetProcessName(int processId)
        {
            try
            {
                Process p = Process.GetProcessById(processId);
                return p.ProcessName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetStateType(int stateId)
        {
            switch (stateId)
            {
                case 1: return "Closed";
                case 2: return "Listen";
                case 3: return "SynSent";
                case 4: return "SynReceived";
                case 5: return "Established";
                case 6: return "FinWait1";
                case 7: return "FinWait2";
                case 8: return "CloseWait";
                case 9: return "Closing";
                case 10: return "LastAck";
                case 11: return "TimeWait";
                case 12: return "DeleteTCB";
                case 100: return "Bound";    // no official documentation found
                default: return "Unknown";
            }
        }

        public override CheckResult Run()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("ROOT\\StandardCIMV2", "SELECT CreationTime, InstanceID, LocalAddress, LocalPort, OwningProcess, RemoteAddress, RemotePort, State FROM MSFT_NetTCPConnection");
            ManagementObjectCollection connections = searcher.Get();

            int index = 0;
            var data = new CheckResult();
            Item[] items = new Item[connections.Count];

            foreach (ManagementObject mo in connections)
            {

                int pid = Convert.ToUInt32(mo["OwningProcess"]);
                int state = Convert.ToUInt8(mo["State"]);
                int creationTime = (int)DateTime.ParseExact(Convert.ToString(mo["CreationTime"]).Substring(0, 14), "yyyyMMddHHmmss", null).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                items[index++] = new Item
                {
                    ["name"] = mo["InstanceID"],
                    ["CreationTime"] = creationTime,
                    ["LocalAddress"] = mo["LocalAddress"],
                    ["LocalPort"] = Convert.ToUInt16(mo["LocalPort"]),
                    ["RemoteAddress"] = mo["RemoteAddress"],
                    ["RemotePort"] = Convert.ToUInt16(mo["RemotePort"]),
                    ["OwningProcessID"] = pid,
                    ["OwningProcess"] = GetProcessName(pid),
                    ["State"] = GetStateType(state),
                };
            }

            data.AddType("netstat", items);
            return data;
        }
    }
}
