namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductProfileTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductProfiles",
                c => new
                    {
                        ProductProfileId = c.Int(nullable: false, identity: true),
                        VoteCount = c.Int(nullable: false),
                        TotalScore = c.Double(nullable: false),
                        ViewCount = c.Int(nullable: false),
                        Sales = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductProfileId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProductProfiles");
        }
    }
}
