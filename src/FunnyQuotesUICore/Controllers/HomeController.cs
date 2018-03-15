using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUICore.Controllers
{
    public class HomeController : Controller
    {
        private readonly DiscoveryHttpClientHandler _handler;

        public HomeController(IDiscoveryClient client, ILoggerFactory logFactory)
        {
            _handler = new DiscoveryHttpClientHandler(client, logFactory.CreateLogger<DiscoveryHttpClientHandler>());
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FunnyQuotes()
        {
            var data = await GetClient().GetStringAsync("http://FunnyQuotesServicesOwin/api/FunnyQuotes/random");
            var result = JsonConvert.DeserializeObject<string>(data);
            return View("Index", result);
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient(_handler, false);
            return client;
        }
    }
}