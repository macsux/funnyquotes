using FunnyQuotesCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FunnyQuotesBasicWindows
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
            services.AddSingleton<LocalFunnyQuoteService>();
            
            services.AddOptions();
            services.Configure<FunnyQuotesConfiguration>(Configuration.GetSection("FunnyQuotes")); // adds typed configuration object and map it to a section of config
            services.AddTransient<IFunnyQuoteService, LocalFunnyQuoteService>();

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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}