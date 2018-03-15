using System;
using System.Data;
using System.Data.Entity;
using System.Web;
using Autofac;
using Autofac.Integration.Wcf;
using Autofac.Integration.Web;
using FortunesCookieDatabase;
using Microsoft.Extensions.Configuration;
using Pivotal.Discovery.Client;
using Steeltoe.CloudFoundry.ConnectorAutofac;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace FortunesLegacyService
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
            //            ConfigurationRootExtensions.RegisterConfig("development", (env, configBuilder) => 
            //                configBuilder.SetBasePath(env.ContentRootPath)
            //                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            //                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            //                .AddCloudFoundry()
            //                .AddEnvironmentVariables());

//            ILoggerFactory logFactory = new LoggerFactory();
//            logFactory.AddConsole(minLevel: LogLevel.Debug);
            var env = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT") ?? "development";
            var config = new ConfigurationBuilder()
                //                configBuilder.SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{env}.json", true)
                .AddEnvironmentVariables()
                .AddCloudFoundry()
                .Build();

            //
            //            var services = new ServiceCollection();
            //            services.AddDiscoveryClient(ConfigurationRootExtensions.Configuration);
            //            services.AddDbContext<FortuneCookieDbContext>(ConfigurationRootExtensions.Configuration);
            //            services.AddMySqlConnection(ConfigurationRootExtensions.Configuration);
            //
            //            var builder = new ContainerBuilder();
            //            builder.Populate(services);
            var builder = new ContainerBuilder();
            builder.RegisterOptions();
            builder.RegisterDiscoveryClient(config);
            builder.RegisterLogging(config);
            builder.RegisterMySqlConnection(config);
            builder.Register(ctx =>
            {
                var connString = ctx.Resolve<IDbConnection>().ConnectionString;
                return new FortuneCookieDbContext(connString);
            });
            builder.RegisterType<FortuneServiceWCF>();
            var container = builder.Build();

            // ensure that discovery client component starts up
            container.StartDiscoveryClient();
            // force db opeartion so db gets created on startup
            container.Resolve<FortuneCookieDbContext>().FortuneCookies.Load();
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