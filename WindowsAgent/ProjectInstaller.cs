using System.ComponentModel;
using System.ServiceProcess;

namespace WindowsAgent
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            // Set the account properties for the service process.
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            // Set the installation properties for the service.
            // The ServiceInstaller.ServiceName must match the
            // ServiceBase.ServiceName set in the service
            // implementation that is installed by this installer.
            serviceInstaller.ServiceName = "InfraSonarAgent";
            serviceInstaller.DisplayName = "InfraSonar Windows Agent";
            serviceInstaller.Description = "This service is used by InfraSonar for collecting monitoring data. See https://docs.infrasonar.com/collectors/agents/windows/ for more infromation.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.DelayedAutoStart = true;

            // Add the installers to the Installer collection.
            Installers.Add(serviceInstaller);
            Installers.Add(serviceProcessInstaller);
        }
    }
}
