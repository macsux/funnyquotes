using System.Data.Entity.Migrations;
using System.Linq;
using FunnyQuotesCommon;

namespace FunnyQuotesCookieDatabase.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<FunnyQuotesCookieDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(FunnyQuotesCookieDbContext context)
        {
            if (!context.FunnyQuotes.Any())
                foreach (var quote in LocalFunnyQuoteService.Quotes)
                    context.FunnyQuotes.AddOrUpdate(x => x.Quote, new FunnyQuote {Quote = quote});
        }
    }
}