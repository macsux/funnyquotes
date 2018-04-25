using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using FunnyQuotesCookieDatabase;

namespace FunnyQuotesServicesOwin.Controllers
{
    public class HealthController : ApiController
    {
        
  

        [HttpGet]
        public string Health()
        {
            return string.Empty;
        }
    }
}