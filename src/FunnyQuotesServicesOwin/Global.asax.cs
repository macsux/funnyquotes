using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;

namespace FunnyQuotesServicesOwin
{
    // not used in OWIN - see Startup.cs
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            
        }

    }
}