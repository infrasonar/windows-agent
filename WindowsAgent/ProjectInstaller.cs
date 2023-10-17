using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace WindowsAgent
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

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
            serviceInstaller.AfterInstall += new InstallEventHandler(StartService);

            // Add the installers to the Installer collection.
            Installers.Add(serviceInstaller);
            Installers.Add(serviceProcessInstaller);
        }

        private void StartService(object sender, InstallEventArgs e)
        {
            ServiceInstaller serviceInstaller = GetServiceInstaller(sender);
            // Start the service after it is installed.
            if (serviceInstaller != null && serviceInstaller.StartType == ServiceStartMode.Automatic)
            {
                var serviceController = new ServiceController(serviceInstaller.ServiceName);
                serviceController.Start();
            }
        }

        private static ServiceInstaller GetServiceInstaller(object sender)
        {
            return sender as ServiceInstaller;
        }
    }

}
