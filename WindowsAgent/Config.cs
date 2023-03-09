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
        readonly String token;

        public Config()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\InfraSonar\\Windows Agent", false))
                {
                    var s = key?.GetValue("Token") as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        this.token = s;
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
