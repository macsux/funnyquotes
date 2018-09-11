using System;
using System.Data;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using FunnyQuotesCommon;
using FunnyQuotesCookieDatabase;
using FunnyQuotesServicesOwin;
using FunnyQuotesServicesOwin.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Pivotal.Discovery.Client;
using Pivotal.Extensions.Configuration.ConfigServer;
using Steeltoe.CloudFoundry.ConnectorAutofac;
using Steeltoe.Common.Configuration.Autofac;
using Steeltoe.Common.Diagnostics;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Logging;
using Steeltoe.Management.Census.Trace;
using Steeltoe.Management.EndpointAutofac;
using Steeltoe.Management.EndpointOwin.Diagnostics;
using Steeltoe.Management.Tracing;
using Steeltoe.Security.Authentication.CloudFoundry;

[assembly: OwinStartup(typeof(Startup))]

namespace FunnyQuotesServicesOwin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            try
            {
                var httpConfig = new HttpConfiguration(); 
                var env = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT") ?? "development"; // standard variable in asp.net core for environment declaration
                
                
                var config = new ConfigurationBuilder() // asp.net core config provider
                    .AddJsonFile("appsettings.json", false, false)
                    .AddJsonFile($"appsettings.{env}.json", true)
                    .AddEnvironmentVariables()
                    .AddCloudFoundry() // maps VCAP environmental variables as a config provider
                    .AddConfigServer() // adds spring config server as a config provider
                    .Build();
                var funnyQuotesConfig = new FunnyQuotesConfiguration();
                config.GetSection("FunnyQuotes").Bind(funnyQuotesConfig); 
                
                // -- container registration 
                var builder = new ContainerBuilder(); // build up autofac container
                builder.RegisterOptions(); // allow injection of strongly typed config
                builder.RegisterDiscoveryClient(config); // register eureka service discovery
                builder.RegisterConfiguration(config);
                builder.RegisterCloudFoundryOptions(config);
                builder.RegisterMySqlConnection(config);
                builder.Register(ctx => // register EF context
                {
                    var connString = ctx.Resolve<IDbConnection>().ConnectionString;
                    return new FunnyQuotesCookieDbContext(connString);
                });
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly()); // register all controllers to be injectable
                builder.RegisterWebApiFilterProvider(httpConfig); // register autofac support for webapi filters
                builder.RegisterType<LoggerExceptionFilterAttribute>() // register global exception handler
                    .AsWebApiExceptionFilterFor<ApiController>()
                    .SingleInstance();
                builder.RegisterCloudFoundryActuators(config);

                var container = builder.Build(); // compile the container
                
                // -- configure owin server
                httpConfig.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html")); // default to json instead of xml
                httpConfig.Routes.MapHttpRoute( // setup default routing for WebApi2
                    "DefaultApi",
                    "api/{controller}/{action}"
                );
                httpConfig.Routes.MapHttpRoute("Health", 
                    "{controller}/{action}", 
                    new { controller = "health", action = "health" }); // map "/" to basic health endpoint so the default PCF health check for HTTP 200 response is satisfied 
                httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container); // assign autofac to provide dependency injection on controllers
                
                // -- setup app pipeline
                app.UseAutofacMiddleware(container); // allows injection of dependencies into owin middleware
                if(funnyQuotesConfig.EnableSecurity)
                    app.UseCloudFoundryJwtBearerAuthentication(config); // add security integration for PCF SSO
                else
                    app.Use<NoAuthenticationMiddleware>(); // dummy security provider which is necessary if you have secured actions on controllers 
                app.UseAutofacWebApi(httpConfig); // merges owin pipeline with autofac request lifecycle
                app.UseWebApi(httpConfig); // standard OWIN WebAPI2
                app.UseCors(CorsOptions.AllowAll);
                container.StartDiscoveryClient(); // ensure that discovery client is started
                container.StartActuators();
                container.Resolve<IDiagnosticsManager>().Start();
                
                var logger = container.Resolve<ILogger<Startup>>();
                logger.LogInformation(">> App Started <<");
            }
            catch (Exception e)
            {
                // given that logging is DI controlled it may not be initialized, just write directly to console
                Console.Error.WriteLine(e);
                throw;
            }
        }
    }
}