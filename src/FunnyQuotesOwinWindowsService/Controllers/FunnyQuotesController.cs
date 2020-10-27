using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using FunnyQuotesCookieDatabase;
using Microsoft.Extensions.Logging;

namespace FunnyQuotesOwinWindowsService.Controllers
{
    [Authorize]
    public class FunnyQuotesController : ApiController
    {
        private readonly Func<FunnyQuotesCookieDbContext> _contextFactory;
        private readonly ILogger<FunnyQuotesController> _logger;

        // GET api/values 
        public FunnyQuotesController(Func<FunnyQuotesCookieDbContext> contextFactory, ILogger<FunnyQuotesController> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<IList<string>> GetAllFunnyQuotes()
        {
            return await _contextFactory().FunnyQuotes.Select(x => x.Quote).ToListAsync();
        }


        [HttpGet]
        [ActionName("random")]
        public async Task<string> GetRandom()
        {
            Console.WriteLine("Control method");
            _logger.LogTrace("Getting random quotes");
            var funnyQuotes = await GetAllFunnyQuotes();
            
            var random = new Random().Next(0, funnyQuotes.Count() - 1);
            _logger.LogDebug($"Total of {funnyQuotes.Count} found in db");
            return funnyQuotes[random];
        }
    }
}