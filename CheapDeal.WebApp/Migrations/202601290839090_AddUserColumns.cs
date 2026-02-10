namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserColumns : DbMigration
    {
        public override void Up()
        {            
            Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Accounts]') AND name = 'CreatedDate')
                BEGIN
                    ALTER TABLE [dbo].[Accounts] 
                    ADD [CreatedDate] DATETIME NOT NULL 
                    CONSTRAINT DF_Accounts_CreatedDate DEFAULT GETDATE()
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Accounts]') AND name = 'IsActive')
                BEGIN
                    ALTER TABLE [dbo].[Accounts] 
                    ADD [IsActive] BIT NOT NULL 
                    CONSTRAINT DF_Accounts_IsActive DEFAULT 1
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Accounts]') AND name = 'LastLoginDate')
                BEGIN
                    ALTER TABLE [dbo].[Accounts] 
                    ADD [LastLoginDate] DATETIME NULL
                END
            ");
        }
        
        public override void Down()
        {
            Sql(@"
                IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Accounts_CreatedDate')
                    ALTER TABLE [dbo].[Accounts] DROP CONSTRAINT DF_Accounts_CreatedDate

                IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Accounts_IsActive')
                    ALTER TABLE [dbo].[Accounts] DROP CONSTRAINT DF_Accounts_IsActive

                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Accounts]') AND name = 'LastLoginDate')
                    ALTER TABLE [dbo].[Accounts] DROP COLUMN [LastLoginDate]

                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Accounts]') AND name = 'IsActive')
                    ALTER TABLE [dbo].[Accounts] DROP COLUMN [IsActive]

                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Accounts]') AND name = 'CreatedDate')
                    ALTER TABLE [dbo].[Accounts] DROP COLUMN [CreatedDate]
            ");
        }
    }
}
