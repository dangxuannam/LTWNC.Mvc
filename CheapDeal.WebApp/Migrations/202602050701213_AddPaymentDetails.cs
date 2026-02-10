namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        PaymentMethod = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransactionId = c.String(maxLength: 100),
                        PaymentInfo = c.String(maxLength: 500),
                        CreatedDate = c.DateTime(nullable: false),
                        PaidDate = c.DateTime(),
                        Notes = c.String(maxLength: 1000),
                        ResponseData = c.String(maxLength: 2000),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Orders", t => t.OrderId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.PaymentTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderCode = c.String(),
                        CustomerName = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.String(nullable: false, maxLength: 50),
                        Status = c.String(),
                        PaymentDate = c.DateTime(nullable: false),
                        TransactionId = c.String(),
                        Note = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "OrderId", "dbo.Orders");
            DropIndex("dbo.Payments", new[] { "OrderId" });
            DropTable("dbo.PaymentTransactions");
            DropTable("dbo.Payments");
        }
    }
}
