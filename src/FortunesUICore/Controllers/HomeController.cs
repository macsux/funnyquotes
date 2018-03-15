using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;

namespace FortunesUICore.Controllers
{
    public class HomeController : Controller
    {
        private readonly DiscoveryHttpClientHandler _handler;
//        private readonly DiscoveryUrlResolver _resolver;


        public HomeController(IDiscoveryClient client, ILoggerFactory logFactory)
        {
            _handler = new DiscoveryHttpClientHandler(client, logFactory.CreateLogger<DiscoveryHttpClientHandler>());
        }

        public IActionResult Index()
        {
//            ViewBag.ServiceUrl = _resolver.LookupService("https://FortunesService/")
//                .ToString();
//            //                .Replace("http://","https://");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Fortune()
        {
            var data = await GetClient().GetStringAsync("http://FortunesService/api/fortunes/random");
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