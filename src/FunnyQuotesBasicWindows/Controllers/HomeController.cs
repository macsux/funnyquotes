using FunnyQuotesCommon;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FunnyQuotesBasicWindows.Controllers
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

        
        public async Task<IActionResult> GetQuote()
        {

            ViewBag.Provider = _client.GetType().FullName;
            var result = await _client.GetQuoteAsync();
            
            return View("Index", result);

        }

        [HttpGet]
        public IActionResult Kill()
        {
            Environment.Exit(-1);
            return Ok();
        }
        

        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction(nameof(Index), "Home");
        }

        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index), "Home");
        }
    }
}