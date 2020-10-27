using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Autofac;
using Microsoft.Extensions.Logging;

namespace FunnyQuotesCookieDatabase
{
    public static class ContainerExtensions
    {
        public static void MigrateDatabase(this IContainer container)
        {
            var logger = container.Resolve<ILoggerFactory>().CreateLogger("Startup.DatabaseMigrator");
            var db = container.Resolve<FunnyQuotesCookieDbContext>();
            var migrator = new DbMigrator(new FunnyQuotesCookieDatabase.Migrations.Configuration(), db);
            // dynamic databaseCreator = Type.GetType("System.Data.Entity.Migrations.Utilities.DatabaseCreator, EntityFramework");
            bool dbExists = db.Database.Exists();

            // var pendingMigrations = (dbExists ? migrator.GetPendingMigrations() : migrator.GetLocalMigrations()).ToList();
            var pendingMigrations =  migrator.GetPendingMigrations().ToList();
            // var appliedMigrations = migrator.GetDatabaseMigrations().ToList();
            if (!pendingMigrations.Any())
            {
                logger.LogInformation("Database is up to date");
                return;
            }
            if(!dbExists)
                logger.LogInformation($"Creating database '{db.Database.Connection.Database}'");
            logger.LogInformation("Applying the following migrations:");

            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation($"- {migration}");
            }
            
            logger.LogInformation("Database successfully migrated");
        }
    }
}