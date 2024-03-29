﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Volume : Check
    {
        private const int _defaultInterval = 5;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "volume";  // Check key.
        private bool _includeShadowVolume  = false;  // Do not include Shadow Volume type as this does not yet work as expected

        public override string Key() { return _key; }
        public override int DefaultInterval() { return _defaultInterval; }
        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            int index = 0;
            var data = new CheckResult();
            DriveInfo[] drives = DriveInfo.GetDrives();
            Item[] items = new Item[drives.Length];
            foreach (DriveInfo drive in drives)
            {
                var item = new Item
                {
                    ["name"] = drive.Name.ToLower(),
                    ["IsReady"] = drive.IsReady,
                    ["DriveType"] = (string)drive.DriveType.ToString(),
                    ["RootDirectory"] = (string)drive.RootDirectory.ToString(),
                };
                if (drive.IsReady)
                {
                    decimal percentUsed = 100 - (drive.TotalFreeSpace / (decimal)drive.TotalSize * 100);
                    item["TotalSize"] = drive.TotalSize;
                    item["DriveFormat"] = drive.DriveFormat;
                    item["TotalFreeSpace"] = drive.TotalFreeSpace;
                    item["VolumeLabel"] = drive.VolumeLabel;
                    item["Percentused"] = percentUsed;
                }
                items[index++] = item;
            }

            data.AddType("volume", items);
            if (_includeShadowVolume)
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Volume, AllocatedSpace, MaxSpace, UsedSpace FROM Win32_ShadowStorage");
                try
                {
                    ManagementObjectCollection result = searcher.Get();
                    index = 0;
                    Item[] shadowVolumes = new Item[result.Count];
                    foreach (ManagementBaseObject mo in result)
                    {
                        ManagementObject moVolume = new ManagementObject();
                        ManagementPath moVolumePath = new ManagementPath((string)mo["Volume"]);
                        moVolume.Path = moVolumePath;
                        moVolume.Get();

                        shadowVolumes[index++] = new Item
                        {
                            ["name"] = Convert.ToString(moVolume.GetPropertyValue("Name")).ToLower(),
                            ["MaxSpace"] = Convert.ToUInt64(mo["MaxSpace"]),
                            ["AllocatedSpace"] = Convert.ToUInt64(mo["AllocatedSpace"]),
                            ["UsedSpace"] = Convert.ToUInt64(mo["UsedSpace"]),
                        };
                    }
                    data.AddType("shadow", shadowVolumes);
                }
                catch (ManagementException ex)
                {
                    _includeShadowVolume = false;
                    Logger.Write(string.Format("Failed retrieve shadow volume(s); {0}; No longer check for shadow volumes until a restart", ex.Message), EventLogEntryType.Warning, EventId.InitializationFailureShadowVolume);
                }
                catch (Exception ex)
                {
                    Logger.Write(string.Format("Failed retrieve shadow volume(s); {0}", ex.Message), EventLogEntryType.Warning, EventId.None);
                }
            }
            return data;
        }
    }
}
