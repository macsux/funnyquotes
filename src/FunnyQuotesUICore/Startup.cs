using System;
using System.Linq;
using FunnyQuotesCommon;
using FunnyQuotesServicesOwin.Authentication;
using FunnyQuotesUICore.Clients;
using FunnyQuotesUICore.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Pivotal.Discovery.Client;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint.Health;
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
        public void ConfigureServices(IServiceCollection services)
        {
            var funnyquotesConfig = new FunnyQuotesConfiguration();
            Configuration.GetSection("FunnyQuotes").Bind(funnyquotesConfig);
            ((IConfigurationRoot)Configuration).AutoRefresh(TimeSpan.FromSeconds(10));
            services.AddMvc();
            services.AddCloudFoundryActuators(Configuration);
            services.AddHystrixMetricsStream(Configuration);
            services.AddDiscoveryClient(Configuration);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<LocalFunnyQuoteService>();
            services.AddScoped<RestFunnyQuotesClient>();
            services.AddOptions();
            services.Configure<FunnyQuotesConfiguration>(Configuration.GetSection("FunnyQuotes"));
            
            services.AddTransient<IFunnyQuoteService>(provider =>
            {
                var config = provider.GetService<IOptionsSnapshot<FunnyQuotesConfiguration>>();
                var implType = config.Value.ClientType;
                if (implType == "rest")
                    return provider.GetService<RestFunnyQuotesClient>();
                return provider.GetService<LocalFunnyQuoteService>();

            });


            var authBuilder = services
                .AddAuthentication((options) =>
                {
                    options.DefaultScheme = CloudFoundryDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
                })
                .AddCookie((options) => { options.AccessDeniedPath = new PathString("/Home/AccessDenied"); });
                
            services.AddAuthorization(options =>
            {
                options.AddPolicy("useridentity", policy => policy.RequireClaim("scope","openid"));
            
            });
            if (funnyquotesConfig.EnableSecurity)
            {
                authBuilder.AddCloudFoundryOAuth(Configuration);
            }
            else
            {
                authBuilder.AddNoSecurity();
                services.NoAuthorization();

            }

            //                .AddCloudFoundryOAuth(Configuration);
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
            app.UseCloudFoundryActuators();
            app.UseHystrixRequestContext();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseHystrixMetricsStream();
            app.UseDiscoveryClient();
        }

    }
}