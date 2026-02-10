namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccountLogin : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageId = c.Int(nullable: false, identity: true),
                        SenderId = c.String(nullable: false, maxLength: 128),
                        ReceiverId = c.String(nullable: false, maxLength: 128),
                        Subject = c.String(nullable: false, maxLength: 500),
                        Content = c.String(nullable: false),
                        SentDate = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadDate = c.DateTime(),
                        IsDeletedBySender = c.Boolean(nullable: false),
                        IsDeletedByReceiver = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MessageId)
                .ForeignKey("dbo.Accounts", t => t.ReceiverId)
                .ForeignKey("dbo.Accounts", t => t.SenderId)
                .Index(t => t.SenderId)
                .Index(t => t.ReceiverId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        AccountId = c.String(nullable: false, maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 200),
                        Content = c.String(nullable: false, maxLength: 1000),
                        Type = c.String(maxLength: 50),
                        Link = c.String(maxLength: 200),
                        IsRead = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ReadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Accounts", t => t.AccountId)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.UserActivities",
                c => new
                    {
                        ActivityId = c.Int(nullable: false, identity: true),
                        AccountId = c.String(nullable: false, maxLength: 128),
                        ActivityType = c.String(nullable: false, maxLength: 100),
                        Details = c.String(maxLength: 500),
                        Timestamp = c.DateTime(nullable: false),
                        IpAddress = c.String(maxLength: 50),
                        UserAgent = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.ActivityId)
                .ForeignKey("dbo.Accounts", t => t.AccountId)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserActivities", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Notifications", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Messages", "SenderId", "dbo.Accounts");
            DropForeignKey("dbo.Messages", "ReceiverId", "dbo.Accounts");
            DropIndex("dbo.UserActivities", new[] { "AccountId" });
            DropIndex("dbo.Notifications", new[] { "AccountId" });
            DropIndex("dbo.Messages", new[] { "ReceiverId" });
            DropIndex("dbo.Messages", new[] { "SenderId" });
            DropTable("dbo.UserActivities");
            DropTable("dbo.Notifications");
            DropTable("dbo.Messages");
        }
    }
}
