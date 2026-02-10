namespace CheapDeal.WebApp.Migrations
{
    using CheapDeal.WebApp.DAL;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CheapDeal.WebApp.DAL.ShopDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(CheapDeal.WebApp.DAL.ShopDbContext context)
        {
            AccountSeeder.Seed(context);
            CategorySeeder.Seed(context);
            SupplierSeeder.Seed(context);
            ShipperSeeder.Seed(context);
            ProductSeeder.Seed(context);
            ShipperRateSeeder.Seed(context);
            context.SaveChanges();
        }
    }
}
