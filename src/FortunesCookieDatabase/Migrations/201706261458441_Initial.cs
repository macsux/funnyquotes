using System.Data.Entity.Migrations;

namespace FortunesCookieDatabase.Migrations
{
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.FortuneCookies",
                    c => new
                    {
                        Id = c.Int(false, true),
                        Cookie = c.String(unicode: false),
                        Language = c.String(unicode: false)
                    })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.FortuneCookies");
        }
    }
}