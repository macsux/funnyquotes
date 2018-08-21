using System;
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using FunnyQuotesCommon;
using FunnyQuotesCookieDatabase;
using FunnyQuotesServicesOwin;
using FunnyQuotesServicesOwin.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Pivotal.Discovery.Client;
using Pivotal.Extensions.Configuration.ConfigServer;
using Steeltoe.CloudFoundry.ConnectorAutofac;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Extensions.Configuration.CloudFoundry;

[assembly: OwinStartup(typeof(Startup))]

namespace FunnyQuotesServicesOwin
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            try
            {
//                var vcap = File.ReadAllText(@"C:\Projects\FunnyQuotes\src\FunnyQuotesServicesOwin\vcap.json");
//                Environment.SetEnvironmentVariable("VCAP_SERVICES",vcap);
                var env = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT") ?? "development";
                var config = new ConfigurationBuilder()
                    //                configBuilder.SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", false, false)
                    .AddJsonFile($"appsettings.{env}.json", true)
                    .AddEnvironmentVariables()
                    .AddCloudFoundry()
                    .AddConfigServer()
                    .Build();
                var funnyQuotesConfig = new FunnyQuotesConfiguration();
                config.GetSection("FunnyQuotes").Bind(funnyQuotesConfig);
                // register DI
                var builder = new ContainerBuilder();
                builder.RegisterOptions();
                
                builder.RegisterDiscoveryClient(config);
                builder.RegisterLogging(config);
                builder.RegisterMySqlConnection(config);
                builder.Register(ctx =>
                {
                    var connString = ctx.Resolve<IDbConnection>().ConnectionString;
                    return new FunnyQuotesCookieDbContext(connString);
                });

                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                var container = builder.Build();

                // assign autofac to provide dependency injection on controllers
                appBuilder.UseAutofacMiddleware(container);
                if(funnyQuotesConfig.EnableSecurity)
                    appBuilder.AddCloudFoundryJwtBearer(config);
                else
                    appBuilder.Use<NoAuthenticationMiddleware>();
                // Configure Web API for self-host. 
                var httpConfig = new HttpConfiguration();
                // default to json
                httpConfig.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
                httpConfig.Routes.MapHttpRoute(
                    "DefaultApi",
                    "api/{controller}/{action}"
                );
                httpConfig.Routes.MapHttpRoute("Health", "{controller}/{action}", new { controller = "health", action = "health" });

                httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);
                appBuilder.UseWebApi(httpConfig);
                appBuilder.UseCors(CorsOptions.AllowAll);
                // ensure that discovery client is started
                container.StartDiscoveryClient();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
        }
    }
}