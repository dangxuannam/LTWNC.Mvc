using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.DAL
{
    public class ProductSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            context.Products.AddOrUpdate(
                p => p.ProductCode,
                new Product
                {
                    ProductId = 1,
                    Name = "Áo thun chân váy đỏ",
                    Alias = "ao-thun-nu-basic-cotton",
                    ProductCode = "ATN-001",
                    ThumbImage = "~/Images/ATCD.jpg",
                    QtyPerUnit = "1 cái",
                    ShortIntro = "Áo thun nữ chất cotton 100%, form basic dễ mặc.",
                    Description = "Áo thun nữ thiết kế đơn giản, chất liệu cotton thoáng mát, thấm hút mồ hôi tốt. Phù hợp mặc hàng ngày, đi làm, đi chơi.",
                    Price = 129000,
                    Quantity = 150,
                    Discount = 0.1f,
                    SupplierId = 1,
                    ProductProfileId = 1,
                    Actived = true 
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Váy đầm dạ hội sang trọng",
                    Alias = "vay-dam-da-hoi-sang-trong",
                    ProductCode = "VDH-002",
                    ThumbImage = "~/Images/DSMHQ.jpg",
                    QtyPerUnit = "1 cái",
                    ShortIntro = "Váy đầm dạ hội thiết kế sang trọng, quý phái.",
                    Description = "Váy đầm dạ hội với chất liệu vải cao cấp, đường may tinh tế. Phù hợp cho các buổi tiệc, sự kiện quan trọng.",
                    Price = 890000,
                    Quantity = 50,
                    Discount = 0.15f,
                    SupplierId = 1,
                    ProductProfileId = 2,
                    Actived = true
                },
                new Product
                {
                    ProductId = 3,
                    Name = "Áo sơ mi nữ công sở",
                    Alias = "ao-so-mi-nu-cong-so",
                    ProductCode = "ASM-003",
                    ThumbImage = "~/Images/aonu.jpg",
                    QtyPerUnit = "1 cái",
                    ShortIntro = "Áo sơ mi nữ form chuẩn, phù hợp môi trường văn phòng.",
                    Description = "Áo sơ mi nữ thiết kế thanh lịch, chất liệu vải kate cao cấp, không nhăn. Mang lại vẻ chuyên nghiệp cho phái đẹp.",
                    Price = 245000,
                    Quantity = 100,
                    Discount = 0.05f,
                    SupplierId = 1,
                    ProductProfileId = 3,
                    Actived = true
                },
                new Product
                {
                    ProductId = 4,
                    Name = "Bộ đồ đường phố cho nữ ",
                    Alias = "bo-do-duong-pho-cho-nu",
                    ProductCode = "AKC-004",
                    ThumbImage = "~/Images/BDDPCB.jpg",
                    QtyPerUnit = "1 cái",
                    ShortIntro = "Set đồ có đường may chắc chắn chất vải dày dặn,đàn hồi tốt.",
                    Description = "Set đồ có chất vải mềm mại, thoáng mát. Thiết kế hiện đại, phù hợp mùa hè,cà phê với bạn bè .",
                    Price = 320000,
                    Quantity = 80,
                    Discount = 0.12f,
                    SupplierId = 1,
                    ProductProfileId = 4,
                    Actived = true 
                },
                new Product
                {
                    ProductId = 5,
                    Name = "Áo thun nam thể thao",
                    Alias = "ao-thun-nam-the-thao",
                    ProductCode = "ATTT-005",
                    ThumbImage = "~/Images/aothethao.jpg",
                    QtyPerUnit = "1 cái",
                    ShortIntro = "Áo thun nam chất liệu thể thao, thoáng khí.",
                    Description = "Áo thun thể thao nam với công nghệ vải dry-fit, thoát ẩm nhanh. Phù hợp tập gym, chạy bộ, các hoạt động thể thao.",
                    Price = 189000,
                    Quantity = 120,
                    Discount = 0.08f,
                    SupplierId = 2,
                    ProductProfileId = 5,
                    Actived = true
                },
                new Product
                {
                    ProductId = 6,
                    Name = "Quần jean nam slimfit",
                    Alias = "quan-jean-nam-slimfit",
                    ProductCode = "QJN-006",
                    ThumbImage = "~/Images/quanjeannam.jpg",
                    QtyPerUnit = "1 cái",
                    ShortIntro = "Quần jean nam co giãn 4 chiều, form slimfit.",
                    Description = "Quần jean nam chất denim cao cấp, co giãn tốt. Form slimfit ôm dáng, tôn vóc dáng nam giới.",
                    Price = 399000,
                    Quantity = 90,
                    Discount = 0.2f,
                    SupplierId = 2,
                    ProductProfileId = 6,
                    Actived = true
                },
                new Product
                {
                    ProductId = 7,
                    Name = "Giày thể thao nam running",
                    Alias = "giay-the-thao-nam-running",
                    ProductCode = "GTT-007",
                    ThumbImage = "~/Images/running_man.jpg",
                    QtyPerUnit = "1 đôi",
                    ShortIntro = "Giày chạy bộ nam, đế êm ái, chống sốc tốt.",
                    Description = "Giày thể thao nam thiết kế năng động, đế cao su chống trơn trượt. Phù hợp chạy bộ, đi bộ, tập gym.",
                    Price = 650000,
                    Quantity = 70,
                    Discount = 0.1f,
                    SupplierId = 3,
                    ProductProfileId = 7,
                    Actived = true
                },
                new Product
                {
                    ProductId = 8,
                    Name = "Vest công sở nữ",
                    Alias = "vest-cong-so-nu",
                    ProductCode = "VCS-008",
                    ThumbImage = "~/Images/vestcongsonu.jpg",
                    QtyPerUnit = "1 bộ",
                    ShortIntro = "Vest nữ công sở thanh lịch, chuyên nghiệp.",
                    Description = "Bộ vest nữ thiết kế hiện đại, chất liệu vải cao cấp. Mang lại vẻ sang trọng, chuyên nghiệp cho phái đẹp nơi công sở.",
                    Price = 780000,
                    Quantity = 40,
                    Discount = 0.15f,
                    SupplierId = 1,
                    ProductProfileId = 8,
                    Actived = true
                },
                new Product
                {
                    ProductId = 9,
                    Name = "Đồ bộ thể thao nam",
                    Alias = "do-bo-the-thao-nam",
                    ProductCode = "DBTT-009",
                    ThumbImage = "~/Images/dothethaonam.jpg",
                    QtyPerUnit = "1 bộ",
                    ShortIntro = "Bộ đồ thể thao nam gồm áo + quần, chất liệu cao cấp.",
                    Description = "Bộ đồ thể thao nam với chất liệu thấm hút mồ hôi, co giãn 4 chiều. Phù hợp tập gym, chơi thể thao, mặc nhà.",
                    Price = 459000,
                    Quantity = 85,
                    Discount = 0.18f,
                    SupplierId = 2,
                    ProductProfileId = 9,
                    Actived = true
                },
                new Product
                {
                    ProductId = 10,
                    Name = "Túi xách nữ thời trang",
                    Alias = "tui-xach-nu-thoi-trang",
                    ProductCode = "TXN-010",
                    ThumbImage = "~/Images/tuixachnu.jpg",
                    QtyPerUnit = "1 cái",
                    ShortIntro = "Túi xách nữ da PU cao cấp, thiết kế sang trọng.",
                    Description = "Túi xách nữ chất liệu da PU bền đẹp, nhiều ngăn tiện dụng. Phù hợp đi làm, đi chơi, dự tiệc.",
                    Price = 289000,
                    Quantity = 110,
                    Discount = 0.1f,
                    SupplierId = 4,
                    ProductProfileId = 10,
                    Actived = true  
                }
            );

            context.SaveChanges();
        }
    }
}