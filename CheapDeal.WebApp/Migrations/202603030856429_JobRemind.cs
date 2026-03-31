namespace CheapDeal.WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class JobRemind : DbMigration
    {
        public override void Up()
        {
            Sql(@"
            DECLARE @pkName NVARCHAR(200)
            SELECT @pkName = tc.CONSTRAINT_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
            WHERE tc.TABLE_NAME = 'Reminders' 
              AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'

            IF @pkName IS NOT NULL
                EXEC('ALTER TABLE [dbo].[Reminders] DROP CONSTRAINT [' + @pkName + ']')

            IF NOT EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Reminders' AND COLUMN_NAME = 'ReminderId'
            )
            BEGIN
                ALTER TABLE [dbo].[Reminders] ADD [ReminderId] INT IDENTITY(1,1) NOT NULL
            END

            IF NOT EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                WHERE TABLE_NAME = 'Reminders' AND CONSTRAINT_TYPE = 'PRIMARY KEY'
            )
            BEGIN
                ALTER TABLE [dbo].[Reminders] 
                ADD CONSTRAINT [PK_dbo.Reminders] PRIMARY KEY ([ReminderId])
            END

            IF EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Reminders' AND COLUMN_NAME = 'Id'
            )
            BEGIN
                ALTER TABLE [dbo].[Reminders] DROP COLUMN [Id]
            END
        ");
        }

        public override void Down()
        {
            Sql(@"
            DECLARE @pkName NVARCHAR(200)
            SELECT @pkName = tc.CONSTRAINT_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
            WHERE tc.TABLE_NAME = 'Reminders' 
              AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'

            IF @pkName IS NOT NULL
                EXEC('ALTER TABLE [dbo].[Reminders] DROP CONSTRAINT [' + @pkName + ']')

            IF NOT EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Reminders' AND COLUMN_NAME = 'Id'
            )
            BEGIN
                ALTER TABLE [dbo].[Reminders] ADD [Id] INT IDENTITY(1,1) NOT NULL
                ALTER TABLE [dbo].[Reminders] 
                ADD CONSTRAINT [PK_dbo.Reminders] PRIMARY KEY ([Id])
            END

            IF EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Reminders' AND COLUMN_NAME = 'ReminderId'
            )
            BEGIN
                ALTER TABLE [dbo].[Reminders] DROP COLUMN [ReminderId]
            END
        ");
        }
    }
}