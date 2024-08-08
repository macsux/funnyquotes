using System;
using System.ServiceProcess;

namespace FunnyQuotesOwinWindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {

            try
            {
                var service = new Service1();
                var launchAsConsole = args.Length > 0 ? (args[0] == "console") : Environment.UserInteractive;
                if (launchAsConsole)
                {
                    Console.WriteLine("=== Starting app as Console === ");
                    service.Start();
                    Console.ReadLine();
                    service.Stop();
                }
                else
                {
                    Console.WriteLine("=== Starting app as Windows Service === ");
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new Service1()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
    }
}
