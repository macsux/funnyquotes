using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FunnyQuotesCommon;
using FunnyQuotesCookieDatabase;

namespace FunnyQuotesLegacyService
{
    public class FunnyQuoteserviceWCF : IFunnyQuoteservice
    {
        private readonly Func<FunnyQuotesCookieDbContext> _dbContextFactory;

        public FunnyQuoteserviceWCF(Func<FunnyQuotesCookieDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public FunnyQuotesCookieDbContext DbContext { get; set; }

        public async Task<string> GetCookieAsync()
        {
            var context = _dbContextFactory();
            var cookies = await context.FunnyQuotes.Select(x => x.Cookie).ToListAsync();
            var randomCookieIndex = new Random().Next(0, cookies.Count - 1);
            return cookies[randomCookieIndex];
        }

        public string GetCookie()
        {
            var context = _dbContextFactory();
            var cookies = context.FunnyQuotes.Select(x => x.Cookie).ToList();
            var randomCookieIndex = new Random().Next(0, cookies.Count - 1);
            return cookies[randomCookieIndex];
        }
    }
}