using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.DAL
{
    public class PictureSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            context.Picture.AddOrUpdate(
                p => p.PictureId,

                new Picture
                {
                    PictureId = 1,
                    Caption = "Áo thun chân váy đỏ - Mặt trước",
                    Path = "/Images/ATCD.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 1
                },
                new Picture
                {
                    PictureId = 2,
                    Caption = "Áo thun chân váy đỏ - Chi tiết chất liệu",
                    Path = "/Images/ATCD-detail.jpg",
                    OrderNo = 2,
                    Actived = true,
                    ProductId = 1
                },

                new Picture
                {
                    PictureId = 3,
                    Caption = "Váy đầm dạ hội - Toàn cảnh",
                    Path = "/Images/DSMHQ.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 2
                },
                new Picture
                {
                    PictureId = 4,
                    Caption = "Váy đầm dạ hội - Chi tiết đường may",
                    Path = "/Images/DSMHQ-detail.jpg",
                    OrderNo = 2,
                    Actived = true,
                    ProductId = 2
                },

                new Picture
                {
                    PictureId = 5,
                    Caption = "Áo sơ mi nữ công sở thanh lịch",
                    Path = "/Images/aonu.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 3
                },
                new Picture
                {
                    PictureId = 6,
                    Caption = "Bộ đồ đường phố cá tính",
                    Path = "/Images/BDDPCB.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 4
                },

                new Picture
                {
                    PictureId = 7,
                    Caption = "Áo thun thể thao nam với công nghệ dry-fit",
                    Path = "/Images/aothethao.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 5
                },

                new Picture
                {
                    PictureId = 8,
                    Caption = "Quần jean nam slimfit co giãn 4 chiều",
                    Path = "/Images/quanjeannam.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 6
                },

                new Picture
                {
                    PictureId = 9,
                    Caption = "Giày chạy bộ nam đế êm ái chống sốc",
                    Path = "/Images/running_man.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 7
                },

                new Picture
                {
                    PictureId = 10,
                    Caption = "Vest công sở nữ thanh lịch chuyên nghiệp",
                    Path = "/Images/vestcongsonu.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 8
                },

                new Picture
                {
                    PictureId = 11,
                    Caption = "Bộ đồ thể thao nam thấm hút mồ hôi tốt",
                    Path = "/Images/dothethaonam.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 9
                },

                new Picture
                {
                    PictureId = 12,
                    Caption = "Túi xách nữ da PU cao cấp sang trọng",
                    Path = "/Images/tuixachnu.jpg",
                    OrderNo = 1,
                    Actived = true,
                    ProductId = 10
                }
            );

            context.SaveChanges();
        }
    }
}