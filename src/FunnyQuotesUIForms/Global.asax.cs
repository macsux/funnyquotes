using System;
using System.Net;
using System.Web;
using Autofac;
using Autofac.Integration.Web;
using FunnyQuotesCommon;
using FunnyQuotesUIForms.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pivotal.Discovery.Client;
using Pivotal.Extensions.Configuration.ConfigServer;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;


namespace FunnyQuotesUIForms
{
    public class Global : HttpApplication, IContainerProviderAccessor
    {
        private static IContainerProvider _containerProvider;

        public IContainerProvider ContainerProvider => _containerProvider;

        private void Application_Start(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            ILoggerFactory logFactory = new LoggerFactory();
            logFactory.AddConsole(LogLevel.Debug);
            var env = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT") ?? "development";
            var config = new ConfigurationBuilder()
//                configBuilder.SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{env}.json", true)
                .AddConfigServer(env, logFactory)
                .AddEnvironmentVariables()
                .Build()
                .AutoRefresh(TimeSpan.FromSeconds(10));
            
            var builder = new ContainerBuilder();
            builder.RegisterOptions();
            builder.RegisterOption<FunnyQuotesConfiguration>(config.GetSection("FunnyQuotes"));
            builder.RegisterDiscoveryClient(config);
            builder.RegisterLogging(config);
            builder.RegisterConsoleLogging();
            builder.RegisterHystrixMetricsStream(config);
            // used for wcf integration

            builder.RegisterType<AsmxFunnyQuotesClient>().Named<IFunnyQuoteService>("asmx");
            builder.RegisterType<WcfFunnyQuotesClient>().Named<IFunnyQuoteService>("wcf");
            builder.RegisterType<LocalFunnyQuoteService>().Named<IFunnyQuoteService>("local");
            builder.RegisterType<RestFunnyQuotesClient>().Named<IFunnyQuoteService>("rest");
            // register service factory
            builder.Register(c =>
            {
                var quotesConfig = c.Resolve<IOptionsSnapshot<FunnyQuotesConfiguration>>();
                return c.ResolveNamed<IFunnyQuoteService>(quotesConfig.Value.ClientType);
            });
            
            var container = builder.Build();
            container.StartDiscoveryClient();
            container.StartHystrixMetricsStream();
            _containerProvider = new ContainerProvider(container);
            Console.WriteLine(">> FunnyQuotesLegacyUI Started<<");
        }

        private void Application_Error(object sender, EventArgs e)
        {
            var exc = Server.GetLastError();
            Console.Error.WriteLine(exc);
        }
    }

    public class FunnyQuotesConfiguration
    {
        public string ClientType { get; set; }
    }
}