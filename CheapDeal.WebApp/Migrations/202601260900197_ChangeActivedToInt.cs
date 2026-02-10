namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeActivedToInt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductHistories", "OriginalProduct", c => c.String(storeType: "ntext"));
            AlterColumn("dbo.Products", "Actived", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "Actived", c => c.Boolean(nullable: false));
            DropColumn("dbo.ProductHistories", "OriginalProduct");
        }
    }
}
