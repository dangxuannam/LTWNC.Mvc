namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserProfileColumns : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                -- Thêm cột FullName
                IF NOT EXISTS (SELECT * FROM sys.columns 
                              WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                              AND name = 'FullName')
                BEGIN
                    ALTER TABLE [dbo].[UserProfiles] 
                    ADD [FullName] NVARCHAR(100) NULL
                    PRINT 'Added FullName column'
                END

                -- Thêm cột Phone
                IF NOT EXISTS (SELECT * FROM sys.columns 
                              WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                              AND name = 'Phone')
                BEGIN
                    ALTER TABLE [dbo].[UserProfiles] 
                    ADD [Phone] NVARCHAR(20) NULL
                    PRINT 'Added Phone column'
                END

                -- Thêm cột Birthday
                IF NOT EXISTS (SELECT * FROM sys.columns 
                              WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                              AND name = 'Birthday')
                BEGIN
                    ALTER TABLE [dbo].[UserProfiles] 
                    ADD [Birthday] DATETIME NULL
                    PRINT 'Added Birthday column'
                END

                -- Thêm cột Gender
                IF NOT EXISTS (SELECT * FROM sys.columns 
                              WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                              AND name = 'Gender')
                BEGIN
                    ALTER TABLE [dbo].[UserProfiles] 
                    ADD [Gender] BIT NULL
                    PRINT 'Added Gender column'
                END
            ");
        }
        
        public override void Down()
        {
            Sql(@"
                IF EXISTS (SELECT * FROM sys.columns 
                          WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                          AND name = 'Gender')
                    ALTER TABLE [dbo].[UserProfiles] DROP COLUMN [Gender]

                IF EXISTS (SELECT * FROM sys.columns 
                          WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                          AND name = 'Birthday')
                    ALTER TABLE [dbo].[UserProfiles] DROP COLUMN [Birthday]

                IF EXISTS (SELECT * FROM sys.columns 
                          WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                          AND name = 'Phone')
                    ALTER TABLE [dbo].[UserProfiles] DROP COLUMN [Phone]

                IF EXISTS (SELECT * FROM sys.columns 
                          WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') 
                          AND name = 'FullName')
                    ALTER TABLE [dbo].[UserProfiles] DROP COLUMN [FullName]
            ");
        }
    }
}
