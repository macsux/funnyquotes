using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace FunnyQuotesOwinWindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("=== Starting app === ");
            try
            {
                var service = new Service1();
                var launchAsConsole = args.Length > 0 ? (args[0] == "console") : Environment.UserInteractive;
                if (launchAsConsole)
                {
                    service.Start();
                    Console.ReadLine();
                    service.Stop();
                }
                else
                {
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
