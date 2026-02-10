namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateGit : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Comments", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Comments", "ProductId", "dbo.Products");
            DropIndex("dbo.Comments", new[] { "AccountId" });
            DropIndex("dbo.Comments", new[] { "ProductId" });
            DropTable("dbo.Comments");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 100),
                        Subject = c.String(nullable: false, maxLength: 100),
                        Content = c.String(nullable: false, maxLength: 1000),
                        PostedTime = c.DateTime(nullable: false),
                        Actived = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        ReplyContent = c.String(maxLength: 1000),
                        ReplyTime = c.DateTime(),
                        AccountId = c.String(maxLength: 128),
                        ProductId = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.CommentId);
            
            CreateIndex("dbo.Comments", "ProductId");
            CreateIndex("dbo.Comments", "AccountId");
            AddForeignKey("dbo.Comments", "ProductId", "dbo.Products", "ProductId", cascadeDelete: true);
            AddForeignKey("dbo.Comments", "AccountId", "dbo.Accounts", "Id", cascadeDelete: true);
        }
    }
}
