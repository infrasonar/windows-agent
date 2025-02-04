﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WindowsAgent
{
    internal class Config
    {
        static private readonly string _authorization = ReadAuthorization();
        static private readonly string _apiUrl = ReadApiUrl();
        static private readonly bool _debug = ReadDebug();
        static private readonly bool _localOnly = ReadlocalOnly();
        static private string _assetName = ReadAssetName();
        static private ulong _assetId = ReadAssetId();
        static private long _disabledChecksCacheAge = ReadDisabledChecksCacheAge();

        private const string _APPKEY = @"Software\Cesbit\InfraSonarAgent";
        private const string _CHKKEY = @"Software\Cesbit\InfraSonarAgent\CheckInterval";

        static public void Init()
        {
            try
            {
                Registry.LocalMachine.CreateSubKey(_CHKKEY);

                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, true))
                {
                    if (key.GetValue("ApiUrl") == null)
                    {
                        key.SetValue("ApiUrl", _apiUrl, RegistryValueKind.String);
                    }
                    if (key.GetValue("Token") == null)
                    {
                        key.SetValue("Token", string.Empty, RegistryValueKind.String);
                    }
                    if (key.GetValue("DisabledChecksCacheAge") == null)
                    {
                        key.SetValue("DisabledChecksCacheAge", 900, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to initialize registry: {0}", ex.Message), EventLogEntryType.Warning, EventId.InitRegistry);
            }
            SetAssetId(_assetId);
        }

        static public void SetAssetId(ulong assetId)
        {
            _assetId = assetId;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, true))
                {
                    key.SetValue("AssetId", (long)_assetId, RegistryValueKind.QWord);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to write AssetId to registry: {0}", ex.Message), EventLogEntryType.Error, EventId.AssetIdRegistry);
                throw ex;
            }
        }

        static public void SetAssetName(string assetName)
        {
            _assetName = assetName;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, true))
                {
                    key.SetValue("AssetName", _assetName, RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to write AssetName to registry: {0}", ex.Message), EventLogEntryType.Warning, EventId.AssetNameRegistry);
            }
        }

        static public bool IsDebug()
        {
            return _debug;
        }

        static public bool IsLocalOnly()
        {
            return _localOnly;
        }

        static public string GetAssetName()
        {
            return _assetName;
        }

        static public ulong GetAssetId()
        {
            return _assetId;
        }

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

        static public long DisabledChecksCacheAge()
        {
            return _disabledChecksCacheAge;
        }

        // Return the CheckInterval in Minutes.
        static public int GetCheckInterval(string checkKey, int defaultInterval)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_CHKKEY, false))
                {
                    int interval = Convert.ToInt32(key.GetValue(checkKey, defaultInterval).ToString());
                    if (interval < 0 || interval > 60 * 24)
                    {
                        Logger.Write(string.Format("Invalid interval for {0}; using the default ({1} minutes)", checkKey, defaultInterval), EventLogEntryType.Error, EventId.InvalidInterval);
                        return defaultInterval;
                    }
                    if (interval != defaultInterval)
                    {
                        Logger.Write(string.Format("Changed interval to {0} minute{1} for check {2}", interval, interval == 1 ? "" : "s", checkKey), EventLogEntryType.Information, EventId.None);
                    }
                    return interval;
                }
            }
            catch (Exception)
            {
                return defaultInterval;
            }
        }

        static public void WriteCheckInterval(string checkKey, int interval)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_CHKKEY, true))
                {
                    if (key.GetValue(checkKey) == null)
                    {
                        key.SetValue(checkKey, (uint)interval, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to write registry entry for check ({0}): {1}", checkKey, ex.Message), EventLogEntryType.Warning, EventId.WriteIntervalFailed);
            }
        }

        static private string ReadApiUrl()
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

        static private string ReadAuthorization()
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

        static private string ReadAssetName()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    var s = key?.GetValue("AssetName") as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        return s;
                    }
                }
            }
            catch (Exception) { }
            return System.Net.Dns.GetHostEntry(string.Empty).HostName;
        }

        static private ulong ReadAssetId()
        {
            ulong assetId = 0;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    assetId = Convert.ToUInt64(key.GetValue("AssetId", 0).ToString());
                }
            }
            catch (Exception) { }
            return assetId;
        }

        static private long ReadDisabledChecksCacheAge()
        {
            long disabledChecksCacheAge = 900;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    disabledChecksCacheAge = Convert.ToInt64(key.GetValue("DisabledChecksCacheAge", 900).ToString());
                }
            }
            catch (Exception) { }
            return disabledChecksCacheAge;
        }

        static private bool ReadDebug()
        {
            int debug = 0;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    debug = Convert.ToInt32(key.GetValue("Debug", 0).ToString());
                }
            }
            catch (Exception) { }
            return debug != 0;
        }

        static private bool ReadlocalOnly()
        {
            int localOnly = 0;
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(_APPKEY, false))
                {
                    localOnly = Convert.ToInt32(key.GetValue("LocalOnly", 0).ToString());
                }
            }
            catch (Exception) { }
            return localOnly != 0;
        }
    }
}
