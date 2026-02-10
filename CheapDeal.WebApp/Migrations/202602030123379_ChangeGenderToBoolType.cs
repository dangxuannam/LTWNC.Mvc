namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeGenderToBoolType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserProfiles", "Gender", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserProfiles", "Gender", c => c.String(maxLength: 10));
        }
    }
}
