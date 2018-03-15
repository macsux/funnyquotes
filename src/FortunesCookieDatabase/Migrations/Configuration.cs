using System.Data.Entity.Migrations;
using System.Linq;
using FortunesCommon;

namespace FortunesCookieDatabase.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<FortuneCookieDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(FortuneCookieDbContext context)
        {
            if (!context.FortuneCookies.Any())
                foreach (var cookie in LocalFortuneService.Cookies)
                    context.FortuneCookies.AddOrUpdate(x => x.Cookie, new FortuneCookie {Cookie = cookie, Language = "en"});
        }
    }
}