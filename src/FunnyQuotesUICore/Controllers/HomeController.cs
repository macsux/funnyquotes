using System;
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
        public IActionResult Index()
        {
            ViewBag.Provider = _client.GetType().FullName;
            return View();
        }

        [Authorize]
        [Authorize(Policy = "useridentity")]
        public async Task<IActionResult> GetQuote()
        {

            ViewBag.Provider = _client.GetType().FullName;
            var result = await _client.GetQuoteAsync();
            
            return View("Index", result);

        }

        [HttpGet]
        [Authorize]
        public IActionResult Login()
        {
            return RedirectToAction(nameof(Index), "Home");
        }

//        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult AccessDenied()
        {
            ViewData["Message"] = "Insufficient permissions.";
            return View();
        }
    }
}