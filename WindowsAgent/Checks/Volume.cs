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
                var item = new Item
                {
                    ["name"] = drive.Name.ToLower(),
                    ["IsReady"] = drive.IsReady,
                    ["DriveType"] = (string)drive.DriveType.ToString(),
                    ["RootDirectory"] = (string)drive.RootDirectory.ToString(),
                };
                if (drive.IsReady)
                {
                    item["TotalSize"] = drive.TotalSize;
                    item["DriveFormat"] = drive.DriveFormat;
                    item["TotalFreeSpace"] = drive.TotalFreeSpace;
                    item["VolumeLabel"] = drive.VolumeLabel;
                }
                items[index++] = item;
            }

            data.AddType("volume", items);
            return data;
        }
    }
}
