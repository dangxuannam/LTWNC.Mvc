using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using CheapDeal.WebApp.Models;

namespace CheapDeal.WebApp.DAL
{
    public static class ShipperSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            var shippers = new List<Shipper>
            {
                new Shipper { 
                    CompanyName = "CTCP Vận Tải & Dịch Vụ Phúc Tâm", 
                    Address = "75 Hồ Văn Huê, P. 9, Q. Phú Nhuận, Tp. HCM", 
                    Phone = "083-997-4002" },
                
                new Shipper { CompanyName = "Công ty TNHH SX - TM - DV Long Phan", 
                    Address = "40 C3, Chu Văn An, P. 26, Q. Bình Thạnh, Tp. HCM", 
                    Phone = "090-330-6938" },
                
                new Shipper { CompanyName = "TNHH TMDV Giải Pháp Hàng Hóa Phương Nam",
                    Address = "243 Huỳnh Văn Bánh, TP. Đà Lạt, Lâm Đồng", 
                    Phone = "093-417-1588" },
               
                new Shipper { CompanyName = "Công ty TNHH Thành Bưởi", 
                    Address = "266-268 Lê Hồng Phong, P. 4, Q. 5, Tp. HCM", 
                    Phone = "083-830-1714" },
                
                new Shipper { CompanyName = "TNHH Vận Tải & TM DV Thanh Tâm", 
                    Address = "48 Đường Số 6, P. 9, Q. Gò Vấp, Tp. HCM", 
                    Phone = "028-398-5678" },
                
                new Shipper { CompanyName = "TNHH Vận Tải & TM DV Tuấn Huy", 
                    Address = "48 Đường Số 1, P. 9, Q. Gò Vấp, Tp. HCM", 
                    Phone = "028-321-5678" },
                
                new Shipper { CompanyName = "TNHH Vận Tải & TM DV Hoàng Gia", 
                    Address = "12 Đường Số 3, P. 9, Q. Gò Vấp, Tp. HCM", 
                    Phone = "028-374-5678" },
                
                new Shipper { CompanyName = "TNHH Vận Tải & TM DV Minh Quân", 
                    Address = "22 Xô Viết Nghệ Tĩnh, P. 21, Q. Bình Thạnh, Tp. HCM", 
                    Phone = "028-374-3456" },
                
                new Shipper { CompanyName = "TNHH Vận Tải & TM DV An Phát", 
                    Address = "55 Phan Văn Trị, P. 11, Q. Gò Vấp, Tp. HCM", 
                    Phone = "028-999-8888" },
                
                new Shipper { CompanyName = "TNHH Vận Tải & TM DV Đại Phát", 
                    Address = "99 Lê Văn Sỹ, P. 14, Q. 3, Tp. HCM", 
                    Phone = "028-369-7777" }
            };

            context.Shippers.AddOrUpdate(s => s.CompanyName, shippers.ToArray());
            context.SaveChanges();
        }
    }
}