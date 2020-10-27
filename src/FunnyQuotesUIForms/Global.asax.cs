using System;
using System.Net;
using System.Web;
using Autofac;
using Autofac.Integration.Web;
using FunnyQuotes;
using FunnyQuotesCommon;
using FunnyQuotesUIForms.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Configuration.Autofac;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Extensions.Configuration.ConfigServer;
using Steeltoe.Extensions.Logging;
using Steeltoe.Management.Endpoint.Health.Contributor;


namespace FunnyQuotesUIForms
{
    public class Global : HttpApplication, IContainerProviderAccessor
    {
        private static IContainerProvider _containerProvider;

        public IContainerProvider ContainerProvider => _containerProvider;

        private void Application_Start(object sender, EventArgs e)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                
                var logger = StartupHelper.BootstrapLogger;
                var config = StartupHelper.Configuration;

                var builder = new ContainerBuilder();
                builder.RegisterConfiguration(config);
                builder.RegisterOptions();
                builder.RegisterOption<FunnyQuotesConfiguration>(config.GetSection("FunnyQuotes"));
                builder.RegisterDiscoveryClient(config);
                
                builder.RegisterLogging(config); // allow loggers to be injectable by AutoFac
                builder.RegisterType<DynamicConsoleLoggerProvider>().As<ILoggerProvider>().AsSelf(); // via management endpoints
                builder.RegisterHystrixMetricsStream(config);
                
                builder.RegisterType<DiskSpaceContributor>().As<IHealthContributor>();
                builder.RegisterType<ConfigServerHealthContributor>().As<IHealthContributor>();
                builder.RegisterType<EurekaApplicationsHealthContributor>().As<IHealthContributor>();
                builder.RegisterType<EurekaServerHealthContributor>().As<IHealthContributor>();
                
                // register 4 different implementations of IFunnyQuoteService and assign them unique names
                builder.RegisterType<AsmxFunnyQuotesClient>().Named<IFunnyQuoteService>("asmx");
                builder.RegisterType<WcfFunnyQuotesClient>().Named<IFunnyQuoteService>("wcf");
                builder.RegisterType<LocalFunnyQuoteService>().Named<IFunnyQuoteService>("local");
                builder.RegisterType<RestFunnyQuotesClient>().Named<IFunnyQuoteService>("rest");
                // register dynamic resolution of implementation of IFunnyQuoteService based on named implementation defined in the config
                builder.Register(c =>
                {
                    var quotesConfig = c.Resolve<IOptionsSnapshot<FunnyQuotesConfiguration>>();
                    return c.ResolveNamed<IFunnyQuoteService>(quotesConfig.Value.ClientType);
                });

                var container = builder.Build();
                container.StartDiscoveryClient(); // start eureka client and add current app into the registry
                container.StartHystrixMetricsStream(); // start publishing hystrix stream
                container.StartActuators(); // map routes for actuator endpoints
                _containerProvider = new ContainerProvider(container); // setup autofac WebForms integration
                
                logger.LogInformation(">> FunnyQuotesLegacyUI Started <<");
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                throw;
            }
        }
        


        private void Application_Error(object sender, EventArgs e)
        {
            var logger = ContainerProvider.ApplicationContainer.Resolve<ILogger<Global>>();
            var exception = Server.GetLastError();
            logger.LogError(exception.Message, exception);
        }
    }
}