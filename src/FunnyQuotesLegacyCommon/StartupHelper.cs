using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Autofac;
using FunnyQuotesCommon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;
using Steeltoe.Extensions.Logging;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Hypermedia;

namespace FunnyQuotes
{
    public static class StartupHelper
    {
        
        private static Lazy<ILoggerFactory> _bootstrapLoggerFactory = new Lazy<ILoggerFactory>(() =>
        {
            
            var config = new ConfigurationBuilder()
                .AddLocalConfigFiles(Environment)
                .AddEnvironmentVariables()
                .AddCloudFoundry()
                .Build();
            var containerBuilder = new ContainerBuilder();
            // containerBuilder.RegisterType<DynamicConsoleLoggerProvider>().As<ILoggerProvider>().As<IDynamicLoggerProvider>().AsSelf();
            containerBuilder.RegisterConsoleLogging();
            containerBuilder.RegisterLogging(config);
            containerBuilder.RegisterOptions();
            
            containerBuilder.RegisterOption<ConsoleLoggerOptions>(config);
            var container = containerBuilder.Build();
            var logFactory = container.Resolve<ILoggerFactory>();
            return logFactory;
        });

        private static Lazy<ILogger> _bootstrapLogger = new Lazy<ILogger>(() => _bootstrapLoggerFactory.Value.CreateLogger("Startup"));

        private static Lazy<IConfiguration> _configuration = new Lazy<IConfiguration>(() =>
            new ConfigurationBuilder()
                .AddLocalConfigFiles(Environment)
                .AddInMemoryCollection()
                .AddConfigServer(Environment, _bootstrapLoggerFactory.Value)
                .AddCloudFoundry()
                .AddEnvironmentVariables()
                .Build()
                .AutoRefresh(TimeSpan.FromSeconds(10)));
        public static string Environment => System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        public static ILogger BootstrapLogger => _bootstrapLogger.Value;
        public static IConfiguration Configuration => _configuration.Value;
        
        public static void StartActuators(this IContainer container)
        {
            var configuration = container.Resolve<IConfiguration>();
            var dynamicLogger = container.Resolve<IEnumerable<ILoggerProvider>>().OfType<DynamicConsoleLoggerProvider>().First();
            var healthContributors = container.Resolve<IEnumerable<IHealthContributor>>();
            container.TryResolve<IApiExplorer>(out var apiExplorer);
            var loggerFactory = container.Resolve<ILoggerFactory>();
            ActuatorConfigurator.UseAllActuators(configuration, dynamicLogger, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry, healthContributors, apiExplorer, loggerFactory);
        }

        
    }
}