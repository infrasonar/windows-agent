using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;


namespace WindowsAgent.Checks
{
    using Item = Dictionary<string, object>;

    internal class Volume : Check
    {
        private const int _defaultInterval = 60;  // Interval in minutes, can be overwritten with REG key.
        private const string _key = "volume";  // Check key.        

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
                items[index] = new Item
                {
                    ["name"] = drive.Name.ToLower(),
                    ["IsReady"] = drive.IsReady,
                    ["TotalSize"] = drive.TotalSize,
                    ["DriveFormat"] = drive.DriveFormat,
                    ["DriveType"] = (string)drive.DriveType.ToString(),
                    ["RootDirectory"] = (string)drive.RootDirectory.ToString(),
                    ["TotalFreeSpace"] = drive.TotalFreeSpace,
                    ["VolumeLabel"] = drive.VolumeLabel,
                };
                index++;
            }

            data.AddType("volume", items);
            return data;
        }
    }
}
