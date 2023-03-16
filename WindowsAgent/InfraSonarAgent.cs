using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace WindowsAgent
{
    public partial class InfraSonarAgent : ServiceBase
    {
        public const string CollectorKey = "windows"; 
        private static string _version;
        private static ulong _assetId;
        private static string _assetName;
        private static Checks.System _systemCheck = new Checks.System();

        public InfraSonarAgent()
        {
            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            _version = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MajorRevision);
        }

        public static string GetVersion() { return _version; }

        public static string GetAssetname() { return _assetName; }
        
        public static ulong GetAssetId() { return _assetId; }

        protected override void OnStart(string[] args)
        {            
            if (Config.HasToken() == false)
            {
                Logger.Write("No token found; Set the HKLM\\Software\\Cesbit\\InfraSonarAgent\\Token registry key", EventLogEntryType.Error, EventId.TokenNotFound);
            }
            else
            {
                _systemCheck.Start();
            }
        }

        protected override void OnStop()
        {
        }
    }
}
