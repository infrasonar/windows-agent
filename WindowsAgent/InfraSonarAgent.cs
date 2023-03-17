using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace WindowsAgent
{
    public partial class InfraSonarAgent : ServiceBase
    {
        public const string CollectorKey = "windows";
        public const string AssetKind = "Windows";
        private static string _version;
        private static Checks.System _systemCheck = new Checks.System();

        public InfraSonarAgent()
        {
            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            _version = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MajorRevision);
        }

        public static string GetVersion() { return _version; }

        protected override void OnStart(string[] args)
        {
            try { Config.Init(); } catch { Stop(); /* Event log is written on failure */ }

            if (Config.GetAssetId() == 0)
            {
                if (string.IsNullOrWhiteSpace(Config.GetAssetName()))
                {
                    Logger.Write("Missing asset name", EventLogEntryType.Error, EventId.AssetNameNotFound);
                    Stop();
                }

                Task.Run(Announce.CreateAsset).Wait();
            }
            if (Config.HasToken() == false)
            {
                Logger.Write("No token found; Set the HKLM\\Software\\Cesbit\\InfraSonarAgent\\Token registry key", EventLogEntryType.Error, EventId.TokenNotFound);
                Stop();
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
