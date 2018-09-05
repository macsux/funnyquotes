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
                foreach (var quote in LocalFunnyQuoteService.Quotes)
                    context.FunnyQuotes.AddOrUpdate(x => x.Quote, new FunnyQuote {Quote = quote});
        }
    }
}