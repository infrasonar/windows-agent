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
        static private String _authorization = _readAuthorization();
        static private string _apiUrl = _readApiUrl();

        private const string _APPKEY = "Software\\Cesbit\\InfraSonarAgent";
        private const string _CHKKEY = "Software\\Cesbit\\InfraSonarAgent\\Checks";

        static public string GetAuthorization()
        {
            return _authorization;
        }

        static public string GetApiUrl()
        {
            return _apiUrl;
        }

        static public bool HasToken()
        {
            return _authorization != null;
        }

        static private string _readApiUrl()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    var s = key?.GetValue("ApiUrl") as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        s = s.TrimEnd('/');
                        return s;
                    }
                }
            }
            catch (Exception) { }
            return "https://api.infrasonar.com";
        }

        static private String _readAuthorization()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    var s = key?.GetValue("Token") as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        return "Bearer " + s;
                    }
                }
            }
            catch (Exception) { }
            return null;
        }

        // Return the CheckInterval in Minutes.
        static public int GetCheckInterval(string checkName, int defaultInterval)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_CHKKEY, false))
                {
                    int interval = Convert.ToInt32(key.GetValue(checkName).ToString());
                    if (interval < 0 || interval > 60*24)
                    {
                        Logger.Write(String.Format("Invalid interval for {0}; using the default ({1}s)", checkName, defaultInterval), EventLogEntryType.Error, EventId.InvalidTimestamp);
                        return defaultInterval;
                    }
                    Logger.Write(String.Format("Using interval {1}s for check {0}", interval, checkName), EventLogEntryType.Information, EventId.CustomTimestamp);
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
