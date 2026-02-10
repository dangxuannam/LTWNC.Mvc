using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using CheapDeal.WebApp.Models;

namespace CheapDeal.WebApp.DAL
{
    public static class ShipperRateSeeder
    {
        public static void Seed(ShopDbContext context)
        {

            ShipperSeeder.Seed(context);

            var ghtk = context.Shippers.FirstOrDefault(s => s.CompanyName.Contains("Phúc Tâm"))?.ShipperID ?? 1;
            var ghn = context.Shippers.FirstOrDefault(s => s.CompanyName.Contains("Long Phan"))?.ShipperID ?? 2;
            var viettel = context.Shippers.FirstOrDefault(s => s.CompanyName.Contains("Thành Bưởi"))?.ShipperID ?? 3;
            var jnt = context.Shippers.FirstOrDefault(s => s.CompanyName.Contains("An Phát"))?.ShipperID ?? 4;

            var rates = new List<ShippingRate>
            {
                new ShippingRate { ShipperId = ghtk, ProvinceName = null, MinWeight = 0.0, MaxWeight = 1.0, Price = 30000m },
                new ShippingRate { ShipperId = ghtk, ProvinceName = null, MinWeight = 1.0, MaxWeight = 3.0, Price = 45000m },
                new ShippingRate { ShipperId = ghtk, ProvinceName = null, MinWeight = 3.0, MaxWeight = 5.0, Price = 60000m },
                new ShippingRate { ShipperId = ghn, ProvinceName = null, MinWeight = 0.0, MaxWeight = 2.0, Price = 35000m },
                new ShippingRate { ShipperId = ghn, ProvinceName = "Hồ Chí Minh", MinWeight = 0.0, MaxWeight = 1.0, Price = 25000m },
                new ShippingRate { ShipperId = ghn, ProvinceName = "Hà Nội", MinWeight = 0.0, MaxWeight = 1.0, Price = 28000m },
                new ShippingRate { ShipperId = viettel, ProvinceName = null, MinWeight = 0.0, MaxWeight = 1.0, Price = 32000m },
                new ShippingRate { ShipperId = viettel, ProvinceName = "Đà Nẵng", MinWeight = 0.0, MaxWeight = 2.0, Price = 40000m },
                new ShippingRate { ShipperId = jnt, ProvinceName = null, MinWeight = 0.0, MaxWeight = 3.0, Price = 38000m },
                new ShippingRate { ShipperId = jnt, ProvinceName = "Lâm Đồng", MinWeight = 0.0, MaxWeight = 1.0, Price = 35000m }
            };

            context.ShippingRates.AddOrUpdate(
                r => new { r.ShipperId, r.ProvinceName, r.MinWeight, r.MaxWeight },
                rates.ToArray()
            );

            context.SaveChanges();
        }
    }
}