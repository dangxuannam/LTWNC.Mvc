using CheapDeal.WebApp.Models;
using Microsoft.SqlServer.Server;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Razor.Generator;

namespace CheapDeal.WebApp.DAL
{
    public class CategorySeeder
    {
        public static void Seed(ShopDbContext context)
        {

            var topCates = new Category[]
            {
                new Category
                {
                    Name = "Thời trang nữ",
                    Alias = "thoi-trang-nu",
                    Description = "Quần áo nữ, váy, tất, giày",
                    IconPath = "~/Images/thoitrangnu.jpg",
                    Actived = true,
                    OrderNo = 1,
                    ParentID = null 
                },
                new Category
                {
                    Name = "Thời trang nam", 
                    Alias = "thoi-trang-nam",
                    Description = "Đồ thể thao, áo, quần, áo khoác nam",
                    IconPath = "~/Images/thoitrangnam.jpg",
                    Actived = true,
                    OrderNo = 2,
                    ParentID = null
                },
                new Category
                {
                    Name = "Phụ kiện",
                    Alias = "phu-kien",
                    Description = "Đồng hồ, ba lô, túi xách, giày dép, trang sức",
                    IconPath = "~/Images/phukien.jpg",
                    Actived = true,
                    OrderNo = 3,
                    ParentID = null
                },
                new Category
                {
                    Name = "Mẹ và bé",
                    Alias = "me-va-be",
                    Description = "Set đồ dành cho bà bầu, mẹ và trẻ nhỏ",
                    IconPath = "~/Images/mevabe.jpg",
                    Actived = true,
                    OrderNo = 4,
                    ParentID = null
                }
            };

            context.Categories.AddOrUpdate(c => c.Alias, topCates);
            context.SaveChanges();

           
            var ttNuId = context.Categories.Single(x => x.Alias == "thoi-trang-nu").CategoryId;
            var ttNamId = context.Categories.Single(x => x.Alias == "thoi-trang-nam").CategoryId;

            var ttNuSubCates = new Category[]
            {
                new Category
                {
                    Name = "Đầm - Váy",
                    Alias = "dam-vay",
                    Description = "Váy, đầm dạ tiệc, đầm dạo phố, đầm công sở",
                    IconPath = "~/Images/vaydanba.jpg",
                    Actived = true,
                    OrderNo = 1,
                    ParentID = ttNuId
                },
                new Category
                {
                    Name = "Áo nữ",
                    Alias = "ao-nu",
                    Description = "Áo thun, áo sơ-mi, áo khoác nữ",
                    IconPath = "~/Images/aonu.jpg",
                    Actived = true,
                    OrderNo = 2,
                    ParentID = ttNuId
                },

                new Category
                {
                    Name = "Vest công sở ",
                    Alias = "vest-cong-so",
                    Description = "Mỗi kiểu áo vest sẽ mang đến cho bạn một phong cách khác nhau, " +
                    "có thể dịu dàng,nữ tính, hay phá cách," +
                    "hiện đại tùy vào sự kết hợp của bạn ",
                    IconPath = "~/Images/vestcongsonu.jpg",
                    Actived = true,
                    OrderNo = 3,
                    ParentID = ttNuId
                }
            };
            context.Categories.AddOrUpdate(c => c.Alias, ttNuSubCates);

            var ttNamSubCates = new Category[]
            {
                new Category
                {
                    Name = "Áo nam",
                    Alias = "ao-nam",
                    Description = "Áo thun, áo sơ-mi, áo khoác nam",
                    IconPath = "~/Images/aothethao.jpg",
                    Actived = true,
                    OrderNo = 1,
                    ParentID = ttNamId
                },
                new Category
                {
                    Name = "Đồ thể thao nam",
                    Alias = "do-the-thao-nam",
                    Description = "Bộ đồ thể thao, phụ kiện dành cho nam",
                    IconPath = "~/Images/dothethaonam.jpg",
                    Actived = true,
                    OrderNo = 2,
                    ParentID = ttNamId
                },

                new Category
                {
                    Name = "Giày thể thao  nam",
                    Alias = "giay-the-thao-nam",
                    Description = "Các loại giày với chất lượng không thể chê vào đâu được ",
                    IconPath = "~/Images/running_man.jpg",
                    Actived = true,
                    OrderNo = 3,
                    ParentID = ttNamId
                }
            };
            context.Categories.AddOrUpdate(c => c.Alias, ttNamSubCates);

            // Lưu tất cả thay đổi cuối cùng
            context.SaveChanges();
        }
    }
}

