using System.Net.Http;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesUICore.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;

namespace FunnyQuotesUICore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunnyQuoteService _client;

        public HomeController(IFunnyQuoteService client)
        {
            _client = client;
        }

        public IActionResult Index()
        {
            ViewBag.Provider = _client.GetType().FullName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FunnyQuotes()
        {
            ViewBag.Provider = _client.GetType().FullName;
            var result = await _client.GetCookieAsync();
            return View("Index", result);
        }

    }
}