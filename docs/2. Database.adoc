== Databases

=== Run App locally and show db initialization via EF code first

. Run MySQL locally via docker

 > docker pull mysql
 > docker run --name mysql -p 3306:3306/tcp -d -e MYSQL_ALLOW_EMPTY_PASSWORD=yes mysql:5.7
 
. Connect to MySQL via CLI 

 > docker run -it --link mysql:mysql --rm mysql sh -c "exec mysql -h$MYSQL_PORT_3306_TCP_ADDR -P$MYSQL_PORT_3306_TCP_PORT -uroot -p$MYSQL_ENV_MYSQL_ROOT_PASSWORD"
 
. Create empty database

 > CREATE DATABASE funnyquotes;
 
. Launch FunnyQuotesServiceOwin locally, either from Visual Studio or Rider (must be on Windows)
. Demonstrate database initialization by app startup by querying the database. From MySQL CLI:

 > USE funnyquotes;
 > SHOW Tables;
 > select * from FunnyQuotes;

. Show migration table and it's content

 > DESCRIBE __MigrationHistory;
 > select MigrationId, ContextKey, ProductVersion From __MigrationHistory;
 
=== Demo creating new migration

. Change `FunnyQuotesCookieDatabase.FunnyQuote` class to add a new field:

 public class FunnyQuote
 {
    public int Id { get; set; }
    public string Quote { get; set; }
    public long Views { get; set; }
 }

. From Visual Studio Package Manager console, run 

 > EntityFramework\Add-Migration -Name ViewCountField -Project FunnyQuotesCookieDatabase -StartUpProject FunnyQuotesLegacyService
 
+
Highlight that startup project argument is used to determine the connection to the database in order to compare the schema. Web.Config must have ConnectionString section even if it's not used in normal course of the application to run this command.
. Show the new migration that was added under `FunnyQuotesCookieDatabase\Migrations`.
. Show that at this point the database is still based on old schema

 > DESCRIBE FunnyQuotes;

. Run FunnyQuotesServiceOwin again and hit endpoint that uses the database to force migration to be applied

 http://localhost:61111/api/funnyquotes/random

. Confirm that new migration is applied on the database.

 > DESCRIBE FunnyQuotes;
 > select MigrationId, ContextKey, ProductVersion From __MigrationHistory;
 
+
Extract column should now be visible and new record in migration table

=== Push to PCF
. Provision a MySQL instance from marketplace. Use `mysql-funnyquotes` as name
. Push Owin backend

 > cd FunnyQuotesServicesOwin
 > cf push FunnyQuotesServicesOwin -s windows2016 -b hwc_buildpack
 
. Bind to MySQL service and restart
. Confirm that everything works by hitting `/api/funnyquotes/random` endpoint
. Open up `FunnyQuotesServicesOwin.Startup` class and explain use of Steeltoe Connectors to initialize db context. Highlight the following lines of code

                builder.RegisterMySqlConnection(config);
                builder.Register(ctx => // register EF context
                {
                    var connString = ctx.Resolve<IDbConnection>().ConnectionString;
                    return new FunnyQuotesCookieDbContext(connString);
                });
                
+
Explain that helper methods exist when registering EF Core, but for EF 6.x IDbConnection get's auto configured, and we can feed it into EF registration as per above

. Push FunnyQuotesLegacyService using manifest and how things can bind to services without needing to restart
