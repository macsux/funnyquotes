using System;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using FunnyQuotes;
using FunnyQuotesCommon;
using FunnyQuotesCookieDatabase;
using FunnyQuotesOwinWindowsService.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Cors;
using Owin;
using Steeltoe.CloudFoundry.ConnectorAutofac;
using Steeltoe.Common.Configuration.Autofac;
using Steeltoe.Common.Logging.Autofac;
using Steeltoe.Common.Options.Autofac;
using Steeltoe.Discovery.Client;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;
using Steeltoe.Management.EndpointOwinAutofac;
using Steeltoe.Security.Authentication.CloudFoundry;

namespace FunnyQuotesOwinWindowsService
{
    public class Startup 
    { 
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder app) 
        { 
            try
            {
                var httpConfig = new HttpConfiguration(); 

                var config = StartupHelper.Configuration;
                // var funnyQuotesConfig = new FunnyQuotesConfiguration();
                
                // config.GetSection("FunnyQuotes").Bind(funnyQuotesConfig); 
                
                // -- container registration 
                var builder = new ContainerBuilder(); // build up autofac container
                builder.RegisterOptions(); // allow injection of strongly typed config
                builder.RegisterOption<FunnyQuotesConfiguration>(config.GetSection("FunnyQuotes"));
                builder.RegisterDiscoveryClient(config); // register eureka service discovery
                builder.RegisterConfiguration(config);
                builder.RegisterCloudFoundryOptions(config);
                builder.RegisterMySqlConnection(config);
                builder.RegisterConsoleLogging();
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
                var funnyQuotesConfig = container.Resolve<IOptionsMonitor<FunnyQuotesConfiguration>>().CurrentValue;
                
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
                ContainerBuilderExtensions.StartActuators(container);
                var logger = container.Resolve<ILogger<Startup>>();
                
                container.MigrateDatabase();
                
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