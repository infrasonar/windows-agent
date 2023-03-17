using System.ServiceProcess;

namespace WindowsAgent
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new InfraSonarAgent()
            };

            ServiceBase.Run(ServicesToRun);
        }
    }
}
