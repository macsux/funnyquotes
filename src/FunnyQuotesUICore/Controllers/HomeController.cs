using System.Net.Http;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesUICore.Clients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
//        [Authorize]
//        [Authorize(Policy = "testgroup")]
        public IActionResult Index()
        {
            ViewBag.Provider = _client.GetType().FullName;
            return View();
        }

        [HttpPost]
        [Authorize]
//        [Authorize(Policy = "testgroup")]
        public async Task<IActionResult> FunnyQuotes()
        {
            ViewBag.Provider = _client.GetType().FullName;
            var result = await _client.GetCookieAsync();
            return View("Index", result);
        }
        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


        [HttpGet]
        [Authorize]
        public IActionResult Login()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

//        public IActionResult Manage()
//        {
//            ViewData["Message"] = "Manage accounts using UAA or CF command line.";
//            return View();
//        }
//
//        public IActionResult AccessDenied()
//        {
//            ViewData["Message"] = "Insufficient permissions.";
//            return View();
//        }
    }
}