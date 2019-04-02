using System.ServiceProcess;

namespace Omen
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase omenService = new OmenDaemon();

            omenService.CanHandlePowerEvent = true;
            omenService.CanPauseAndContinue = true;
            omenService.CanShutdown = true;

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                omenService
            };

            ServiceBase.Run(ServicesToRun);
        }
    }
}
