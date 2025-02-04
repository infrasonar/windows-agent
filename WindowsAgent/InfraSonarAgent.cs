﻿using System;
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
        private static Checks.Software _softwareCheck = new Checks.Software();
        private static Checks.Updates _updatesCheck = new Checks.Updates();
        private static Checks.Volume _volumeCheck = new Checks.Volume();
        private static Checks.Processs _processCheck = new Checks.Processs();
        private static Checks.Network _networkCheck = new Checks.Network();
        private static Checks.Memory _memoryCheck = new Checks.Memory();
        private static Checks.Processor _processorCheck = new Checks.Processor();
        private static Checks.Users _usersCheck = new Checks.Users();
        private static Checks.NtDomain _ntDomainCheck = new Checks.NtDomain();
        private static Checks.Netstat _netstatCheck = new Checks.Netstat();
        private static Checks.Disk _diskCheck = new Checks.Disk();
        private static Checks.Certificate _certificateCheck = new Checks.Certificate();
        private static Checks.Heartbeat _heartbeatCheck = new Checks.Heartbeat();

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
                Logger.Write("No token found; Set the HKLM\\Software\\WOW6432Node\\Cesbit\\InfraSonarAgent\\Token registry key", EventLogEntryType.Error, EventId.TokenNotFound);
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
            _softwareCheck.Start();
            _updatesCheck.Start();
            _volumeCheck.Start();
            _processCheck.Start();
            _networkCheck.Start();
            _memoryCheck.Start();
            _processorCheck.Start();
            _usersCheck.Start();
            _ntDomainCheck.Start();
            _netstatCheck.Start();
            _diskCheck.Start();
            _certificateCheck.Start();
            _heartbeatCheck.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
