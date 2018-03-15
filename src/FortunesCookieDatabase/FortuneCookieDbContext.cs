using System.Data.Entity;
using FortunesCookieDatabase.Migrations;
using MySql.Data.Entity;

namespace FortunesCookieDatabase
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class FortuneCookieDbContext : DbContext
    {
        public FortuneCookieDbContext() : base("name=Fortunes")
        {
        }

        public FortuneCookieDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<FortuneCookieDbContext, Configuration>(true));
        }

        public DbSet<FortuneCookie> FortuneCookies { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FortuneCookie>().HasKey(x => x.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}