using System;
using System.Data;
using System.Data.Entity;
using System.Web;
using Autofac;
using Autofac.Integration.Wcf;
using Autofac.Integration.Web;
using FunnyQuotesCookieDatabase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Pivotal.Discovery.Client;
using Steeltoe.CloudFoundry.ConnectorAutofac;
using Steeltoe.Common.Configuration.Autofac;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Logging;

namespace FunnyQuotesLegacyService
{
    public class Global : HttpApplication, IContainerProviderAccessor
    {
        private static IContainerProvider _containerProvider;

        public IContainerProvider ContainerProvider => _containerProvider;

        public static IDbConnection DatabaseFactory()
        {
            return _containerProvider.RequestLifetime.Resolve<IDbConnection>();
        }

        private void Application_Start(object sender, EventArgs e)
        {
            var env = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT") ?? "development";
            ApplicationConfig.RegisterConfig(env);
            var builder = new ContainerBuilder();
            builder.RegisterConfiguration(ApplicationConfig.Configuration);
            builder.RegisterLogging(ApplicationConfig.Configuration);
            builder.RegisterOptions();
            builder.RegisterDiscoveryClient(ApplicationConfig.Configuration);
            builder.Register(ctx => new DynamicLoggerProvider(new ConsoleLoggerSettings().FromConfiguration(ApplicationConfig.Configuration))) // add SteelToe dynamic logger. works similar to
                .AsSelf()                                                                                                                      // console logger, but allows log levels to be altered 
                .As<ILoggerProvider>()
                .SingleInstance(); 
            builder.RegisterMySqlConnection(ApplicationConfig.Configuration);
            builder.Register(ctx =>
            {
                var connString = ctx.Resolve<IDbConnection>().ConnectionString;
                return new FunnyQuotesCookieDbContext(connString);
            });
            builder.RegisterType<FunnyQuoteServiceWcf>();
            var container = builder.Build();
            container.StartActuators();
            // ensure that discovery client component starts up
            container.StartDiscoveryClient();
            // force db opeartion so db gets created on startup
            container.Resolve<FunnyQuotesCookieDbContext>().FunnyQuotes.Load();
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