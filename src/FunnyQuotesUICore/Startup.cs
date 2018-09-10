using System;
using FunnyQuotesCommon;
using FunnyQuotesUICore.Clients;
using FunnyQuotesUICore.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Pivotal.Discovery.Client;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Tracing;
using Steeltoe.Security.Authentication.CloudFoundry;

namespace FunnyQuotesUICore
{
    public class Startup
    {
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider  ConfigureServices(IServiceCollection services)
        {
            // while we're registering FunnyQuotesConfiguration as part of .Configure call, we need that data now
            // as we're making registration decisions. We manually gonna create an instance and map it on to config
            var funnyquotesConfig = new FunnyQuotesConfiguration();
            Configuration.GetSection("FunnyQuotes").Bind(funnyquotesConfig);
            ((IConfigurationRoot)Configuration).AutoRefresh(TimeSpan.FromSeconds(10)); // start a background timer thread to update config every 10 seconds
                                                                                       // alternatively can do the same thing by POSTing to /refresh endpoint
            services.AddMvc();
            services.AddCloudFoundryActuators(Configuration); // enable all actuators on /cloudfoundryapplication endpoint that integrate with CF with enabled security
            services.AddHystrixMetricsStream(Configuration); // stream metrics telemetry to a hystrix stream
            services.AddDiscoveryClient(Configuration); // register eureka (service discovery) with container. Can inject IDiscoveryClient
            services.AddDistributedTracing(Configuration); //
            services.AddTransient<DiscoveryHttpMessageHandler>(); // used for HttpClientFactory
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // .net core way of accessing current http context (legacy HttpContext.Current)
            services.AddLogging(); // can inject ILogger<T> 
            services.AddSingleton<LocalFunnyQuoteService>();
            services.AddScoped<RestFunnyQuotesClient>();
            
            services.AddOptions();
            services.Configure<FunnyQuotesConfiguration>(Configuration.GetSection("FunnyQuotes")); // adds typed configuration object and map it to a section of config
            services.AddHystrixCommand<RestFunnyQuotesClient.GetQuoteCommand>("Core.RandomQuote", "Core.RandomQuote", Configuration); // register injectable hystrix command
            services.AddTransient<IFunnyQuoteService>(provider =>
            {
                // the concrete implementation of IFunnyQuoteService is based on what's configured in config provider
                // this can change at runtime if config value changes
                var config = provider.GetService<IOptionsSnapshot<FunnyQuotesConfiguration>>();
                var implType = config.Value.ClientType;
                if (implType == "rest")
                    return provider.GetService<RestFunnyQuotesClient>();
                return provider.GetService<LocalFunnyQuoteService>();

            });
            services.AddHttpClient<RestFunnyQuotesClient.GetQuoteCommand>(client =>
            {
                client.BaseAddress = new Uri("http://FunnyQuotesServicesOwin/api/FunnyQuotes/");
            }).AddHttpMessageHandler<DiscoveryHttpMessageHandler>(); // use eureka integration with all HttpClient objects


            
            // add OAuth2 sign in scheme
            var authBuilder = services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CloudFoundryDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
                })
                .AddCookie((options) => { options.AccessDeniedPath = new PathString("/Home/AccessDenied"); });
                
            services.AddAuthorization(options =>
            {
                options.AddPolicy("useridentity", policy => policy.RequireClaim("scope","openid"));
            });
            
            // use CF SSO if security is enabled
            if (funnyquotesConfig.EnableSecurity)
            {
                authBuilder.AddCloudFoundryOAuth(Configuration);
            }
            else
            {
                // if security is disabled, add dummy auth processors.
                // This is necessary because controller's [Authorize] attributes will throw if no auth provider is registered
                authBuilder.AddNoSecurity();
                services.NoAuthorization();

            }
            return services.BuildServiceProvider(validateScopes: true);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseForwardedHeaders(); // correctly configures oauth2 token redirect back to the app by ensuring the public protocol is matched (https vs http).
                                       // this is done by looking at headers, because from apps perspective it always looks like it's http because TLS was terminated earlier
            app.UseCloudFoundryActuators(); // creates the route maps in the MVC stack for actuators
            app.UseHystrixRequestContext(); // allows request context to be accessible within hystrix execution model.
                                            // this is necessary because there's some thread switching happening
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseHystrixMetricsStream(); // start publishing hystrix metric stream 
            app.UseDiscoveryClient(); // start eureka client and connect to the registry server to fetch registry
        }
    }
}