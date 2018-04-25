using System.Data.Entity.Migrations;
using System.Linq;
using FunnyQuotesCommon;

namespace FunnyQuotesCookieDatabase.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<FunnyQuotesCookieDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(FunnyQuotesCookieDbContext context)
        {
            if (!context.FunnyQuotes.Any())
                foreach (var cookie in LocalFunnyQuoteService.Cookies)
                    context.FunnyQuotes.AddOrUpdate(x => x.Cookie, new FunnyQuote {Cookie = cookie, Language = "en"});
        }
    }
}