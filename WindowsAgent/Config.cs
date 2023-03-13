using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal class Config
    {
        static private String _token = _Config();

        private const string _APPKEY = "Software\\Cesbit\\InfraSonarAgent";
        private const string _CHKKEY = "Software\\Cesbit\\InfraSonarAgent\\Checks";

        static public String GetToken()
        {
            return _token;
        }

        static public bool HasToken()
        {
            return _token != null;
        }

        static private String _Config()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    var s = key?.GetValue("Token") as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        return s;
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        // Return the CheckInterval in Seconds.
        static public int GetCheckInterval(string checkName, int defaultInterval)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_CHKKEY, false))
                {
                    int interval = Convert.ToInt32(key.GetValue(checkName).ToString());
                    if (interval < 60 || interval > 3600*24)
                    {
                        Logger.Write(String.Format("Invalid inteval for {0}; using default", checkName), EventLogEntryType.Information, EventId.InvalidTimestamp);
                        return defaultInterval;
                    }
                    return interval;
                }
            }
            catch (Exception) 
            {
                return defaultInterval; 
            }
        }
    }
}
