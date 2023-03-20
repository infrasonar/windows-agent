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
        private static Checks.Services _servicesCheck = new Checks.Services();

        public InfraSonarAgent()
        {
            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            _version = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        public static string GetVersion() { return _version; }

        protected override void OnStart(string[] args)
        {
            try { Config.Init(); } catch { Stop(); /* Event log is written on failure */ }

            if (Config.HasToken() == false)
            {
                Logger.Write("No token found; Set the HKLM\\Software\\Cesbit\\InfraSonarAgent\\Token registry key", EventLogEntryType.Error, EventId.TokenNotFound);
                Stop();
                return;
            }

            if (Config.GetAssetId() == 0)
            {
                if (string.IsNullOrWhiteSpace(Config.GetAssetName()))
                {
                    Logger.Write("Missing asset name", EventLogEntryType.Error, EventId.AssetNameNotFound);
                    Stop();
                    return;
                }

                try
                {
                    Task.Run(Announce.CreateAsset).Wait();
                }
                catch (Exception ex)
                {
                    Logger.Write(string.Format("Failed to create asset; {0}", ex.InnerException.Message), EventLogEntryType.Error, EventId.CreateAsset);
                    Stop();
                    return;
                }                
            }
            else
            {
                try
                {
                    Task.Run(Announce.JoinAsset).Wait();
                }                
                catch (Exception ex)
                {
                    Logger.Write(string.Format("Failed to announce asset; {0}", ex.InnerException.Message), EventLogEntryType.Error, EventId.AnnounceAsset);
                    Stop();
                    return;
                }
            }

            Logger.Write(string.Format("Start InfraSonar {0} collector v{1}, Asset: {2} ({3})", CollectorKey, _version, Config.GetAssetName(), Config.GetAssetId()), EventLogEntryType.Information, EventId.None);
            
            _systemCheck.Start();
            _servicesCheck.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
