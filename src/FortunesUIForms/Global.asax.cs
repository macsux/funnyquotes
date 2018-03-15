using System;
using System.Net;
using System.Web;
using Autofac;
using Autofac.Integration.Web;
using FortunesCommon;
using FortunesUIForms.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pivotal.Discovery.Client;
using Pivotal.Extensions.Configuration.ConfigServer;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;


namespace FortunesUIForms
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
            builder.RegisterOption<FortunesConfiguration>(config.GetSection("fortunes"));
            builder.RegisterDiscoveryClient(config);
            builder.RegisterLogging(config);
            builder.RegisterConsoleLogging();
            // used for wcf integration

            builder.RegisterType<AsmxFortuneClient>().Named<IFortuneService>("asmx");
            builder.RegisterType<WcfFortuneClient>().Named<IFortuneService>("wcf");
            builder.RegisterType<LocalFortuneService>().Named<IFortuneService>("local");
            builder.RegisterType<RestFortuneClient>().Named<IFortuneService>("rest");
            // register service factory
            builder.Register(c =>
            {
                var localContext = c.Resolve<IComponentContext>();
                var fortunesConfig = c.Resolve<IOptionsSnapshot<FortunesConfiguration>>();
                Func<IFortuneService> clientFactory = () => localContext.ResolveNamed<IFortuneService>(fortunesConfig.Value.ClientType);
                return clientFactory;
            });
            // register cookie service (to be resolved out of factory above)
            builder.Register(c =>
            {
                var localContext = c;
                return localContext.Resolve<Func<IFortuneService>>()();
            });
            var container = builder.Build();
            // ensure that discovery client component starts up
//            container.Resolve<IDiscoveryClient>();
            container.StartDiscoveryClient();
            _containerProvider = new ContainerProvider(container);
            Console.WriteLine(">> FortuneLegacyUI Started<<");
        }

        private void Application_Error(object sender, EventArgs e)
        {
            var exc = Server.GetLastError();
            Console.Error.WriteLine(exc);
        }
    }

    public class FortunesConfiguration
    {
        public string ClientType { get; set; }
    }
}