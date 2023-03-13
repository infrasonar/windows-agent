using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal class Config
    {
        private String _token;

        public String GetToken()
        {
            return _token;
        }

        public bool HasToken()
        {
            return _token != null;
        }

        public Config()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("Software\\Cesbit\\InfraSonarAgent", false))
                {
                    var s = key?.GetValue("Token") as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        _token = s;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
