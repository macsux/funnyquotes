using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using FunnyQuotesCookieDatabase;

namespace FunnyQuotesServicesOwin.Controllers
{
    public class FunnyQuotesController : ApiController
    {
        private readonly Func<FunnyQuotesCookieDbContext> _contextFactory;

        // GET api/values 
        public FunnyQuotesController(Func<FunnyQuotesCookieDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IList<string>> GetAllFunnyQuotes()
        {
            return await _contextFactory().FunnyQuotes.Select(x => x.Cookie).ToListAsync();
        }


        [HttpGet]
        [ActionName("random")]
        public async Task<string> GetRandom()
        {
            var FunnyQuotes = await GetAllFunnyQuotes();
            var random = new Random().Next(0, FunnyQuotes.Count() - 1);
            return FunnyQuotes[random];
        }
    }
}