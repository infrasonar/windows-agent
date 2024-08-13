using Microsoft.Win32;
using System;
using System.Collections.Generic;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Software : Check
    {
        private const int _defaultInterval = 60;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "software";  // Check key.        

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            var data = new CheckResult();
            List<Item> items = new List<Item>();
            List<string> names = new List<string>();

            List<string> registry_keys = new List<string>
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (string registry_key in registry_keys)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
                {
                    foreach (string subkey_name in key.GetSubKeyNames())
                    {
                        using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                        {
                            string name = CheckResult.ConvertUnicodeStringToAscii((string)subkey.GetValue("DisplayName"));
                            if (name != null & !names.Contains(name))
                            {
                                names.Add(name);

                                Item item = new Item
                                {
                                    ["name"] = name,
                                    ["Version"] = (string)subkey.GetValue("DisplayVersion"),
                                    ["Publisher"] = (string)subkey.GetValue("Publisher"),
                                    ["UninstallCommand"] = (string)subkey.GetValue("UninstallString"),
                                    ["ModifyPath"] = (string)subkey.GetValue("ModifyPath"),
                                };
                                var installDate2 = subkey.GetValue("InstallDate2");
                                if (installDate2 != null)
                                {
                                    if (installDate2.GetType() == typeof(int))
                                    {
                                        item["InstallDate"] = (int)installDate2;
                                    }
                                }
                                else
                                {
                                    var installDate = subkey.GetValue("InstallDate");
                                    if (installDate != null)
                                    {
                                        if (installDate.GetType() == typeof(string))
                                        {
                                            item["InstallDate"] = (long)DateTime.ParseExact((string)installDate, "yyyyMMdd", null).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                        }
                                        else if (installDate.GetType() == typeof(long))
                                        {
                                            item["InstallDate"] = (long)installDate;
                                        }
                                        else if (installDate.GetType() == typeof(int))
                                        {
                                            item["InstallDate"] = (int)installDate;
                                        }
                                    }
                                }

                                items.Add(item);
                            }
                        }
                    }
                }
            }

            data.AddType("software", items.ToArray());
            return data;
        }
    }
}
