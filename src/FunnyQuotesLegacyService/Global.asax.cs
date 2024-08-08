using System;
using System.Data;
using System.Net;
using System.Web;
using Autofac;
using Autofac.Integration.Wcf;
using Autofac.Integration.Web;
using FunnyQuotes;
using FunnyQuotesCookieDatabase;
using Microsoft.Extensions.Logging;
using static Steeltoe.CloudFoundry.Connector.EF6Autofac.MySqlDbContextContainerBuilderExtensions;
using Steeltoe.CloudFoundry.ConnectorAutofac;
using Steeltoe.Common.Configuration.Autofac;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Discovery.Client;
using Steeltoe.Extensions.Logging;
using Steeltoe.Management.EndpointOwinAutofac;

namespace FunnyQuotesLegacyService
{
    public class Global : HttpApplication, IContainerProviderAccessor
    {
        private static IContainerProvider _containerProvider = null!;

        public IContainerProvider ContainerProvider => _containerProvider;

        public static IDbConnection DatabaseFactory()
        {
            return _containerProvider.RequestLifetime.Resolve<IDbConnection>();
        }

        private void Application_Start(object sender, EventArgs e)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var configuration = StartupHelper.Configuration;
            var builder = new ContainerBuilder();
            
            builder.RegisterConfiguration(configuration);
            builder.RegisterCloudFoundryActuators(configuration);
            builder.RegisterLogging(configuration);
            builder.RegisterType<DynamicConsoleLoggerProvider>().As<ILoggerProvider>();
            builder.RegisterOptions();
            builder.RegisterDiscoveryClient(configuration);
            builder.RegisterMySqlConnection(configuration);
            builder.RegisterMySqlDbContext<FunnyQuotesCookieDbContext>(configuration);
            builder.RegisterType<FunnyQuoteServiceWcf>();
            var container = builder.Build();
            container.StartActuators();
            // container.StartActuators();
            // ensure that discovery client component starts up
            container.StartDiscoveryClient();
            // force db opeartion so db gets created on startup
            // container.Resolve<FunnyQuotesCookieDbContext>().FunnyQuotes.Load();
            container.MigrateDatabase();
            _containerProvider = new ContainerProvider(container);
            AutofacHostFactory.Container = container;

        }

        private void Application_Error(object sender, EventArgs e)
        {
            var exc = Server.GetLastError();
            Console.Error.WriteLine(exc);
        }
    }
}