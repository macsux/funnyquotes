namespace FunnyQuotesCookieDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FunnyQuotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quote = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FunnyQuotes");
        }
    }
}
