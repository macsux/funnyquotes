using System.Web.Http;

namespace FunnyQuotesOwinWindowsService.Controllers
{
    public class HealthController : ApiController
    {
        [HttpGet]
        public object Health() // acts as a default response for "/" endpoint
        {
            return new {Status = "UP"};
        }
    }
}