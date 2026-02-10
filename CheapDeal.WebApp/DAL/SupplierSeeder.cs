using CheapDeal.WebApp.Models;
using System.Data.Entity.Migrations;
using System.Linq;

namespace CheapDeal.WebApp.DAL
{
    public class SupplierSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            context.Suppliers.AddOrUpdate(
                s => s.Name,
                new Supplier
                {
                    Name = "Công ty cổ phần Intimex Việt Nam",
                    ContactName = "Trần Đăng Nguyên",
                    ContactTitle = "Quản lý bán hàng",
                    Description = "Intimex là công ty kinh doanh xuất nhập khẩu dưới hình thức trao đổi hàng hóa nội thương và hợp tác xã với các nước XHCN và một số nước khác.",
                    Address = "96 Trần Hưng Đạo - Hoàn Kiếm - Hà Nội",
                    Email = "info@intimexvietnam.com",
                    Fax = "043-942-3240",
                    HomePage = "http://www.intimexco.com",
                    Phone = "043-942-4250",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Công ty TNHH Samsung Electronics Việt Nam",
                    ContactName = "Choi Joo Ho",
                    ContactTitle = "Tổng Giám đốc",
                    Description = "Chuyên sản xuất và cung ứng các thiết bị điện tử, điện thoại di động và linh kiện công nghệ cao.",
                    Address = "KCN Yên Phong, Bắc Ninh",
                    Email = "contact.sev@samsung.com",
                    Fax = "0222-369-6001",
                    HomePage = "http://samsung.com/vn",
                    Phone = "0222-369-6000",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Công ty CP Sữa Việt Nam (Vinamilk)",
                    ContactName = "Mai Kiều Liên",
                    ContactTitle = "Tổng Giám đốc",
                    Description = "Cung cấp các sản phẩm sữa, thực phẩm dinh dưỡng hàng đầu Việt Nam.",
                    Address = "10 Tân Trào, Tân Phú, Quận 7, TP. HCM",
                    Email = "vinamilk@vinamilk.com.vn",
                    Fax = "028-541-61226",
                    HomePage = "http://vinamilk.com.vn",
                    Phone = "028-541-55555",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Tập đoàn Vingroup",
                    ContactName = "Phạm Nhật Vượng",
                    ContactTitle = "Chủ tịch HĐQT",
                    Description = "Tập đoàn đa ngành tập trung vào Công nghệ, Công nghiệp và Dịch vụ Thương mại.",
                    Address = "Số 7 Đường Bằng Lăng 1, Việt Hưng, Long Biên, Hà Nội",
                    Email = "info@vingroup.net",
                    Fax = "024-397-48490",
                    HomePage = "http://vingroup.net",
                    Phone = "024-397-49350",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Thế Giới Di Động (MWG)",
                    ContactName = "Nguyễn Đức Tài",
                    ContactTitle = "Chủ tịch HĐQT",
                    Description = "Nhà bán lẻ thiết bị di động và điện máy hàng đầu với mạng lưới phủ sóng toàn quốc.",
                    Address = "Lô T2-1.2, Đường D1, Khu Công Nghệ Cao, Quận 9, TP. HCM",
                    Email = "investor@thegioididong.com",
                    Fax = "028-381-25961",
                    HomePage = "http://mwg.vn",
                    Phone = "028-381-25960",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Công ty TNHH Nước Giải Khát Coca-Cola Việt Nam",
                    ContactName = "Peeyush Sharma",
                    ContactTitle = "Giám đốc điều hành",
                    Description = "Sản xuất và phân phối các loại nước giải khát thương hiệu toàn cầu.",
                    Address = "485 Xa Lộ Hà Nội, Thủ Đức, TP. HCM",
                    Email = "contact@coca-cola.com.vn",
                    Fax = "028-389-61559",
                    HomePage = "http://cocacola.com.vn",
                    Phone = "028-389-61000",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Tổng công ty Hàng không Việt Nam (Vietnam Airlines)",
                    ContactName = "Lê Hồng Hà",
                    ContactTitle = "Tổng Giám đốc",
                    Description = "Hãng hàng không quốc gia Việt Nam, cung cấp dịch vụ vận chuyển hàng không chuyên nghiệp.",
                    Address = "200 Nguyễn Sơn, Long Biên, Hà Nội",
                    Email = "vna@vietnamairlines.com",
                    Fax = "024-387-22375",
                    HomePage = "http://vietnamairlines.com",
                    Phone = "024-382-72727",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Công ty CP FPT",
                    ContactName = "Trương Gia Bình",
                    ContactTitle = "Chủ tịch HĐQT",
                    Description = "Công ty dịch vụ công nghệ thông tin và viễn thông lớn nhất Việt Nam.",
                    Address = "Tòa nhà FPT, Duy Tân, Cầu Giấy, Hà Nội",
                    Email = "fpt@fpt.com.vn",
                    Fax = "024-376-87410",
                    HomePage = "http://fpt.com.vn",
                    Phone = "024-376-87300",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Tập đoàn Masan",
                    ContactName = "Danny Le",
                    ContactTitle = "Tổng Giám đốc",
                    Description = "Dẫn đầu trong lĩnh vực hàng tiêu dùng nhanh (FMCG) và bán lẻ tại Việt Nam.",
                    Address = "8 Lê Duẩn, Bến Nghé, Quận 1, TP. HCM",
                    Email = "masan@masangroup.com",
                    Fax = "028-382-74115",
                    HomePage = "http://masangroup.com",
                    Phone = "028-625-63862",
                    Actived = true
                },
                new Supplier
                {
                    Name = "Tổng công ty Thương mại Hà Nội (Hapro)",
                    ContactName = "Nguyễn Hữu Thắng",
                    ContactTitle = "Chủ tịch HĐQT",
                    Description = "Kinh doanh đa ngành nghề, tập trung vào xuất nhập khẩu và phân phối hàng tiêu dùng.",
                    Address = "38-40 Lê Thái Tổ, Hoàn Kiếm, Hà Nội",
                    Email = "hapro@hapro.vn",
                    Fax = "024-382-67984",
                    HomePage = "http://hapro.com.vn",
                    Phone = "024-382-67966",
                    Actived = true
                }
            );
            context.SaveChanges();
        }
    }
}