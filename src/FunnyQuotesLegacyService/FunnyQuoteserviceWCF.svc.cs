using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesCookieDatabase;

namespace FunnyQuotesLegacyService
{
    public class FunnyQuoteServiceWcf : IFunnyQuoteService
    {
        private readonly Func<FunnyQuotesCookieDbContext> _dbContextFactory;

        public FunnyQuoteServiceWcf(Func<FunnyQuotesCookieDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public FunnyQuotesCookieDbContext DbContext { get; set; }

        public async Task<string> GetQuoteAsync()
        {
            var context = _dbContextFactory();
            var cookies = await context.FunnyQuotes.Select(x => x.Quote).ToListAsync();
            var randomCookieIndex = new Random().Next(0, cookies.Count - 1);
            return cookies[randomCookieIndex];
        }

        public string GetQuote()
        {
            var context = _dbContextFactory();
            var cookies = context.FunnyQuotes.Select(x => x.Quote).ToList();
            var randomCookieIndex = new Random().Next(0, cookies.Count - 1);
            return cookies[randomCookieIndex];
        }
    }
}