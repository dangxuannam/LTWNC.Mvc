using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    public class CategoryController : AdminController
    {
        private ShopDbContext db = new ShopDbContext();
        // GET: Adm/Category
        public ActionResult Index()
        {
            var categories = db.Categories
               .Include(c => c.Parent)
               .OrderBy(x => x.ParentID)
               .ThenBy(x => x.OrderNo);
            return View(categories.ToList());
        }

        public ActionResult List()
        {
            var categories = PopulateCategories();
            return View(categories);
        }
        ///<summary>
        ///Phương thức lấy danh sách của nhóm hàng
        ///và gom chúng theo thứ tự phân cấp để hiển thị trên
        ///cây hoặc menu,...
        /// </summary>
        /// <returns>
        /// Trả về các nhóm mặt hàng sau khi đã gom nhóm
        /// Theo thứ tự phân lớp nhóm cha - nhóm con
        /// </returns>
        private List<Category> PopulateCategories()
        {
            //Lấy tất cả các categories
            var allCates = db.Categories
                .OrderBy(x => x.ParentID)
                .ThenBy(x => x.OrderNo)
                .ToList();

            //Chỉ lấy những nhóm mặt hàng cha của tất cả nhóm khác 
            var groupedCates = allCates
                .Where(x => !x.ParentID.HasValue || x.ParentID == 0)
                .ToList();

            // Với mỗi nhóm cha, tìm các nhóm con của nó 
            foreach (var category in groupedCates)
            { 
            AddSubCategory(category, allCates);
            }
            return groupedCates;
            }

        /// <summary>
        /// Phương thức tìm các nhóm con của nhóm categories 
        /// từ danh sách tất cả các nhóm allCates 
        /// </summary>
        /// 
        /// <param name="category">Nhóm mặt hàng cha cần mặt hàng con </param>
        ///  <param name="allCates">Danh sách tất cả nhóm mặt hàng  </param>
        
        private void AddSubCategory (Category category, List<Category> allCates)
        {
            // Tìm các nhóm con của category
            category.ChildCates = allCates
                .Where(x => x.ParentID == category.CategoryId)
                .ToList();

            //Với mỗi nhóm con,gọi đệ quy để tìm các nhóm con của chúng
            foreach (var subCate in category.ChildCates)
            {
                AddSubCategory(subCate, allCates);
            }
        }

        /// <summary>
        /// Phương thức cập nhật lại thứ tự nhóm mặt hàng 
        /// </summary>
        /// <param name="cid">Mã nhóm mặt hàng đổi thứ tự</param>
        /// <param name="pid">Mã nhóm mặt hàng cha</param>
        /// <param name="siblings">Mã nhóm mặt hàng anh em</param>
        /// <returns>Trả về true nếu cập nhật thành công </returns>

        [HttpPost]
        public ActionResult Reorder(int id, int direction)
        {
            var category = db.Categories.Find(id);
            if (category == null) return RedirectToAction("Index");

            // Lấy danh sách anh em (cùng cha) và sắp xếp theo OrderNo hiện tại
            var siblings = db.Categories
                                .Where(x => x.ParentID == category.ParentID)
                                .OrderBy(x => x.OrderNo)
                                .ToList();

            // Tìm vị trí của thằng hiện tại trong đám anh em
            int index = siblings.FindIndex(x => x.CategoryId == id);

            // Xử lý nút UP (Lên)
            if (direction == 0)
            {
                if (index > 0) // Nếu không phải thằng đầu tiên
                {
                    var prev = siblings[index - 1]; // Lấy thằng đứng ngay trên

                    // Đổi số thứ tự (Swap)
                    int temp = category.OrderNo;
                    category.OrderNo = prev.OrderNo;
                    prev.OrderNo = temp;
                }
            }
            // Xử lý nút DOWN (Xuống)
            else
            {
                if (index < siblings.Count - 1) // Nếu không phải thằng cuối cùng
                {
                    var next = siblings[index + 1]; // Lấy thằng đứng ngay dưới

                    // Đổi số thứ tự (Swap)
                    int temp = category.OrderNo;
                    category.OrderNo = next.OrderNo;
                    next.OrderNo = temp;
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Phương thức chuyển 1 danh sách nhóm mặt hàng (đã được gom
        /// nhóm theo kiểu phân cấp cha - con ) thành một danh sách nhóm
        /// mặt hàng mới (không phân cấp ) để hiển thị lên DropDownList,
        /// </summary>
        /// <param name="source">Nhóm mặt hàng có cấu trúc phân cấp </param>
        /// <param name="disableIds ">Mảng ID của những nhóm không được chọn  </param>
        /// <param name="level">Mức của nút con trong cây phân cấp </param>
        /// <param name="result ">Danh sách nhóm mặt hàng mới</param>
        private void ConvertToTreeStructure(IEnumerable<Category> source,
            List<int> disableIds, int level, List<Category> result)
        {
            //Duyệt qua từng nhóm mặt hàng của nút cha
            foreach (var cate in source)
            {
                // Thay đổi tên nhóm bằng cách thêm các dấu 
                var cateName = (level > 0) ? " ".PadLeft(level + 1, '-') : "";
                cateName += cate.Name;

                //Tạo nhóm mặt hàng mới,thêm vào kết quả 
                result.Add(new Category
                {
                    CategoryId = cate.CategoryId,
                    Name = cateName
                }); 

                //Chỉ cho người dùng chọn nhóm cha là nhóm ở mức 0,1
                //Mục đích là không để tạo ra quá nhiều mức con 
                if (level > 1 ) disableIds.Add(cate.CategoryId);

                //Gọi đệ quy để xử lý các mặt hàng con 
                ConvertToTreeStructure(cate.ChildCates, disableIds, level + 1, result);
            }
        }
        /// <summary>
        /// Phương thức chuẩn bị dữ liệu để hiển thị lên DropDownList
        /// </summary>
        /// <param name="category">Nhóm mặt hàng đang được cập nhật </param>

        private void InitFormData(Category category)
        {
            //Lấy tất cả các nhóm chức năng và gom nhóm chúng 
            var groupedCates = PopulateCategories();

            //Tạo 2 danh sách kết  quả 
            var disableIds = new List<int>();
            var categories = new List<Category>();

            //Chuyển danh sách nhóm mặt hàng đã gom nhóm 
            // về lại dạng thông thường để hiển thị lên DropDownList
            ConvertToTreeStructure(groupedCates, disableIds, 0, categories);

            //Tạo danh sách đã chọn làm dữ liệu nguồn cho DropDownList
            if (category.ParentID > 0)
                ViewBag.ParentID = new SelectList(categories,
                        "CategoryId", "Name", category.ParentID, disableIds);
            else
                ViewBag.ParentID = new SelectList(categories,
                    "CategoryId", "Name", (object)null, disableIds);
        }

        /// <summary>
        /// Phương thức kiểm tra tính hợp lệ của tập tin được Uploads 
        /// </summary>
        /// <param name="upload">Tập tin đang được upload</param>
        private void ValidateUploadImage (HttpPostedFileBase upload) 
        {
            var allowImageFileTypes = new[] { ".jpg", ".jpeg", ".gif", ".png" };

            //Kiểm tra tính hợp lệ của tập tin được uploads 
            if (upload != null) 
            {
                //Kiểm tra trường hợp file rỗng 
                if (upload.ContentLength == 0)
                    ModelState.AddModelError("IconPath", "Tập tin không có nội dung");

                //Kiểm tra trường hợp file quá lớn
                if (upload.ContentLength > 10  * 1024 * 1024)
                    ModelState.AddModelError("IconPath", "Dung lượng tập tin quá lớn (>1MB");

                //Lấy phần mở rộng của tên file 
                var imageExt = Path.GetExtension(upload.FileName);

                //Kiểm tra trường hợp upload các file không đúng định dạng cho phép
                if (!allowImageFileTypes.Contains(imageExt))
                    ModelState.AddModelError("IconPath",
                            "Chỉ được phép uploads tập tin JPG,JPEG,PNG và GIF");
            }
        }
                ///<summary>
                ///Phương thức lưu nội dung tập tin xuống thư mục Uploads/icons
                ///và lấy đường dẫn gắn vào thuộc tính IconPath của Category 
                /// </summary>
                /// <param name=upload">Tập tin đang upload và cần được lưu </param>
                /// <param name=category">Nhóm mặt hàng đang được thêm mới hoặc cập nhật</param>
                
                private void SaveUploadFile(HttpPostedFileBase upload,Category category)
        { 
        //Bảo đảm có file hợp lệ mới lưu
        if (upload != null && upload.ContentLength > 0 )
            {
                //Xóa icon cũ ( nếu có )
                if (!string.IsNullOrEmpty(category.IconPath))
                {
                    var oldFilePath = Server.MapPath(category.IconPath);
                    System.IO.File.Delete(oldFilePath);
                }

                //Lấy đường dẫn tuyệt đối của thư mục lưu file 
                var targetFolder = Server.MapPath("~/Uploads/Icons");

                //Tạo tên mới cho tập tin và đường dẫn tuyệt đối để lưu
                var uniqueFileName = DateTime.Now.Ticks + "_" + upload.FileName;
                var targetFilePath = Path.Combine(targetFolder, uniqueFileName);

                //Lưu file 
                upload.SaveAs(targetFilePath);

                //Lấy đường dẫn tương đối của tập tin vừa upload 
                category.IconPath = Path.Combine("~/Uploads/Icons", uniqueFileName);
            }
        }
          
        //GET : Adm/Category/Create 
        public ActionResult Create ()
        {
            var emptyCate = new Category();
            InitFormData(emptyCate); 
            return View(emptyCate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase upload,
            [Bind(Include = "Name,Alias,Description,Actived,ParentID,OrderNo")] Category category)
        {
            ValidateUploadImage(upload);

            if (db.Categories.SingleOrDefault(x => x.Alias == category.Alias) != null)
            {
                ModelState.AddModelError("Alias", "Alias này đã tồn tại trong CSDL.");
            }

            if (ModelState.IsValid)
            {
                try
                {

                    SaveUploadFile(upload, category);

  
                    db.Categories.Add(category);
                    db.SaveChanges(); 

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                    if (upload != null && upload.ContentLength > 0 && !string.IsNullOrEmpty(category.IconPath))
                    {
                        var filePath = Server.MapPath(category.IconPath);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                            // Reset lại đường dẫn để form không hiện hình ảnh lỗi (nếu cần)
                            category.IconPath = null;
                        }
                    }


                    ModelState.AddModelError("", "Lỗi khi thêm mới (Đã xóa hình vừa upload): " + ex.Message);
                }
            }

            InitFormData(category);
            return View(category);
        }
        //GET: Adm/Category/Edit/5
        public ActionResult Edit (int?id)
        {
            if (id  == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null) 
            {
                return HttpNotFound();
            }
            InitFormData(category);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HttpPostedFileBase upload,
    [Bind(Include = "CategoryId,Name,Alias,Description,IconPath,Actived,ParentID,OrderNo,RowVersion")] Category category)
        {
            ValidateUploadImage(upload);


            if (db.Categories.SingleOrDefault(x => x.Alias == category.Alias &&
                                                x.CategoryId != category.CategoryId) != null)
            {
                ModelState.AddModelError("Alias", "Alias đã tồn tại trong CSDL.");
            }

            if (ModelState.IsValid)
            {
                try
                {

                    SaveUploadFile(upload, category);


                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges(); 

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
            
                    if (upload != null && upload.ContentLength > 0 && !string.IsNullOrEmpty(category.IconPath))
                    {
                        var filePath = Server.MapPath(category.IconPath);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    // --- KẾT THÚC BỔ SUNG ---

                    ModelState.AddModelError("", "Lỗi khi cập nhật (Đã dọn dẹp file rác): " + ex.Message);
                }
            }

            InitFormData(category);
            return View(category);
        }
        protected override void Dispose (bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose (disposing);
        }

        protected override bool OnUpdateToggle(string propName, bool value, object[] keys)
        {
            string query = string.Format(
                "UPDATE dbo.Categories SET {0} = @p0 WHERE CategoryId = @p1", propName);

            return db.Database.ExecuteSqlCommand(query,value, keys[0]) > 0; 
        }
    }
}