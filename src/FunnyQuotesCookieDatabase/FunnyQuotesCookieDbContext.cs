using System.Data.Entity;
using FunnyQuotesCookieDatabase.Migrations;
using MySql.Data.EntityFramework;

namespace FunnyQuotesCookieDatabase
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class FunnyQuotesCookieDbContext : DbContext
    {
        public FunnyQuotesCookieDbContext() : base("name=FunnyQuotes")
        {
            
        }

        public FunnyQuotesCookieDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<FunnyQuotesCookieDbContext, Configuration>(true));
        }

        public DbSet<FunnyQuote> FunnyQuotes { get; set; } = null!;


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FunnyQuote>().HasKey(x => x.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}