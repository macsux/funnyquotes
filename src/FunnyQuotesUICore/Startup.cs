using System;
using FunnyQuotesCommon;

using FunnyQuotesUICore.Clients;
using FunnyQuotesUICore.Security;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Steeltoe.Common.Http.Discovery;
using Steeltoe.Discovery.Client;
using Steeltoe.Management.Endpoint.Metrics;
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
        public void  ConfigureServices(IServiceCollection services)
        {
            // while we're registering FunnyQuotesConfiguration as part of .Configure call, we need that data now
            // as we're making registration decisions. We manually going to create an instance and map it on to config
            var funnyquotesConfig = new FunnyQuotesConfiguration();
            Configuration.GetSection("FunnyQuotes").Bind(funnyquotesConfig);
            ((IConfigurationRoot)Configuration).AutoRefresh(TimeSpan.FromSeconds(10)); // start a background timer thread to update config every 10 seconds
                                                                                       // alternatively can do the same thing by POSTing to /refresh endpoint
            services.AddPrometheusActuator();
            services.AddDiscoveryClient(Configuration); // register eureka (service discovery) with container. Can inject IDiscoveryClient
            services.AddTransient<DiscoveryHttpMessageHandler>(); // used for HttpClientFactory
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // .net core way of accessing current http context (legacy HttpContext.Current)
            services.AddLogging(); // can inject ILogger<T> 
            services.AddSingleton<LocalFunnyQuoteService>();
            services.AddScoped<RestFunnyQuotesClient>();
            services.AddScoped<WcfFunnyQuotesClient>();
            
            services.AddOptions();
            services.Configure<FunnyQuotesConfiguration>(Configuration.GetSection("FunnyQuotes")); // adds typed configuration object and map it to a section of config
            services.AddTransient<IFunnyQuoteService>(provider =>
            {
                // the concrete implementation of IFunnyQuoteService is based on what's configured in config provider
                // this can change at runtime if config value changes
                var config = provider.GetService<IOptionsSnapshot<FunnyQuotesConfiguration>>();
                var implType = config.Value.ClientType;
                if (implType == "rest")
                    return provider.GetService<RestFunnyQuotesClient>();
                if (implType == "wcf")
                    return provider.GetService<WcfFunnyQuotesClient>();
                return provider.GetService<LocalFunnyQuoteService>();

            });
            services.AddHttpClient<RestFunnyQuotesClient>(client =>
            {
                client.BaseAddress = new Uri("http://FunnyQuotesOwinWindowsService/api/FunnyQuotes/");
            }).AddHttpMessageHandler<DiscoveryHttpMessageHandler>(); // use eureka integration with all HttpClient objects
            services.AddSingleton<IAuthenticationSchemeProvider, FeatureToggleAuthenticationSchemeProvider>();

            
            // add OAuth2 sign in scheme
            var authBuilder = services
                
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CloudFoundryDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
                })
                .AddCookie((options) => { options.AccessDeniedPath = new PathString("/Home/AccessDenied"); })
                .AddCloudFoundryOAuth(Configuration)
                .Toggleable();
                
            services.AddAuthorization(options =>
            {
                options.AddPolicy("authenticated", policy => policy.RequireClaim("scope","openid"));
                options.AddPolicy("elevated", policy => policy.RequireClaim("scope", "kill"));
            });
            services.AddControllersWithViews();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            // correctly configures oauth2 token redirect back to the app by ensuring the public protocol is matched (https vs http).
            // this is done by looking at headers, because from apps perspective it always looks like it's http because TLS was terminated earlier
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}