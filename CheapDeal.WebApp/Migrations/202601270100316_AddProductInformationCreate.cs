namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductInformationCreate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "Actived", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "Actived", c => c.Int(nullable: false));
        }
    }
}
