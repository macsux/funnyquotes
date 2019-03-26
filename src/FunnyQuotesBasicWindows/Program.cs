using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Pivotal.Extensions.Configuration.ConfigServer;

namespace FunnyQuotesBasicWindows
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseCloudFoundryHosting()
                .UseStartup<Startup>()
                .Build();
        }
    }
}