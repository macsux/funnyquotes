using Autofac;
using Autofac.Integration.Web;
using FunnyQuotesCommon;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace FunnyQuotesBasicWindows
{
    public class Global : HttpApplication
    {
        private static IContainerProvider _containerProvider;

        public IContainerProvider ContainerProvider => _containerProvider;

        void Application_Start(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<LocalFunnyQuoteService>().Named<IFunnyQuoteService>("local");

            var container = builder.Build();

            _containerProvider = new ContainerProvider(container);

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}