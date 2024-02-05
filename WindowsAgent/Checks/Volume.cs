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

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Volume, AllocatedSpace, MaxSpace, UsedSpace FROM Win32_ShadowStorage");
            try
            {
                ManagementObjectCollection result = searcher.Get();
                index = 0;
                Item[] shadowVolumes = new Item[result.Count];
                foreach (ManagementBaseObject mo in result)
                {
                    shadowVolumes[index++] = new Item
                    {
                        ["name"] = mo["Volume"],  // TODO ref?
                        ["MaxSpace"] = mo["MaxSpace"],
                        ["AllocatedSpace"] = mo["AllocatedSpace"],
                        ["UsedSpace"] = mo["UsedSpace"],
                    };
                }
                data.AddType("shadow", shadowVolumes.ToArray());
            }
            catch (Exception ex)
            {
                if (Config.IsDebug())
                {
                    Logger.Write(string.Format("Failed retrieve shadow volume(s); {0}", ex.Message), EventLogEntryType.Warning, EventId.None);
                }
            }

            return data;
        }
    }
}
