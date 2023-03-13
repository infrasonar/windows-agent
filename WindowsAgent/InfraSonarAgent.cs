using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    public partial class InfraSonarAgent : ServiceBase
    {
        private Config config;

        public InfraSonarAgent()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            config = new Config();
            if (config.HasToken() == false)
            {
                Logger.Write("No token found; Set the HKLM\\Software\\Cesbit\\InfraSonarAgent\\Token registry key", EventLogEntryType.Error, EventId.TokenNotFound);
            }
        }

        protected override void OnStop()
        {
        }
    }
}
