using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace WindowsAgent
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            ServiceProcessInstaller simpleServiceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller simpleServiceInstaller = new ServiceInstaller();

            // Set the account properties for the service process.
            simpleServiceProcessInstaller.Account = ServiceAccount.LocalService;

            // Set the installation properties for the service.
            // The ServiceInstaller.ServiceName must match the
            // ServiceBase.ServiceName set in the service
            // implementation that is installed by this installer.
            simpleServiceInstaller.ServiceName = "InfraSonarAgent";
            simpleServiceInstaller.DisplayName = "InfraSonar Windows Agent";
            simpleServiceInstaller.Description = "This service is used by InfraSonar for collecting monitoring data.";
            simpleServiceInstaller.StartType = ServiceStartMode.Automatic;

            // Add the installers to the Installer collection.
            Installers.Add(simpleServiceInstaller);
            Installers.Add(simpleServiceProcessInstaller);
        }
    }
}
