using CheapDeal.WebApp.Areas.Adm.Models;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    public class ProductController : AdminController
    {
        private ShopDbContext db = new ShopDbContext();

        //GET : Adm/Product 
        public ActionResult Index(ProductSearchModel searchModel)
        {
            var products = db.Products.Include(p => p.Supplier);

            if (!string.IsNullOrWhiteSpace(searchModel.Keyword))
            {
                products = products.Where(x =>
                    x.Name.Contains(searchModel.Keyword) ||
                    x.ShortIntro.Contains(searchModel.Keyword) ||
                    x.Description.Contains(searchModel.Keyword));
            }

            products = products.OrderBy(x => x.Name);

            if (!searchModel.PageSize.HasValue)
                searchModel.PageSize = 10;

            if (!searchModel.PageIndex.HasValue)
                searchModel.PageIndex = 1;


            ViewBag.PageSize = new SelectList(new[] { 5, 10, 25, 50, 100 },
                searchModel.PageSize.Value);
            ViewBag.SearchModel = searchModel;
           
            return View(products.ToPagedList(
                searchModel.PageIndex.Value,  
                searchModel.PageSize.Value));
        }

        //GET :Adm/Product/Create 
        public ActionResult Create()
        {
            var createModel = new ProductCreateViewModel();
            InitFormData(createModel);

            return View(createModel);
        }
        /// <summary>
        /// Phương thức kiểm tra alias đăng nhập trong form create 
        /// và edit có bị trùng với alias đã có trong CSDL chưa 
        /// </summary>
        /// <param name="alias">Tên định danh của sản phẩm đang cập nhật </param>
        /// <returns>Trả về true nếu giá trị không bị trùng </returns>
        
        public JsonResult CheckUniqueAlias (string alias, int? productId)
        {
            if (!productId.HasValue) productId = 0;

            var result = db.Products.SingleOrDefault( x =>
            x.Alias == alias  && x.ProductId != productId.Value) == null;

            return Json (result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Phương thức kiểm tra mã số SP đăng nhập trong form create 
        /// và edit có bị trùng mã số đã có trong CSDL hay chưa 
        /// </summary>
        /// <param name="productCode">Mã số sản phẩm đang cập nhật</param>
        /// <returns>Trả về true nếu không bị trùng </returns>
        
        public JsonResult CheckUniqueCode(string productCode , int? productId)
        {
            if (!productId.HasValue) productId = 0;

            var result = db.Products.SingleOrDefault(x =>
                            x.ProductCode == productCode && x.ProductId != productId.Value) == null;

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private void InitFormData(ProductCreateViewModel model)
        {
            var allCates = db.Categories
                .Where(x => x.Actived)
                .OrderBy(x => x.ParentID)
                .ThenBy(x => x.OrderNo)
                .ToList();
            var suppliers = db.Suppliers
                .Where(x => x.Actived)
                .OrderBy(x => x.Name)
                .ToList();
            if (model.SelectedCategories == null)
            {
                model.Categories = new MultiSelectList(allCates, "CategoryId", "Name");
                model.Suppliers = new SelectList(suppliers, "SupplierId", "Name");
            }
            else 
            {
                model.Categories = new MultiSelectList (allCates, "CategoryId",
                    "Name", model.SelectedCategories);
                model.Suppliers = new SelectList(suppliers, 
                      "SupplierId", "Name", model.SupplierId);
            }
        }
        private float EnsureDiscountValue(ProductCreateViewModel model)
        {
            var discount = 0.0f;

            if (model.Discount >= model.Price)
            {
                ModelState.AddModelError("Discount", "Tiền giảm giá không được >= giá bán");
                discount = 0;
            }
            else if (model.Discount >= 100)
                discount = (float)Math.Round(model.Discount / model.Price, 2);
            else if (model.Discount >= 1)
                discount = model.Discount / 100.0f;
            else
                discount = model.Discount;
            return discount;
        }

        /// <summary>
        /// Phương thức lưu nội dung tập tin xuống thư mục Upload/Picture
        /// và lấy đường dẫn gán vào cho thuộc tính ThumbImage của product 
        /// </summary>
        /// <param name="upload">Tập tin upload và cần được lưu</param>
        /// <returns>Trả về đường dẫn của file sau khi upload </returns>

        private string SaveUploadFile(HttpPostedFileBase upload, string oldfilePath)
        {
            //Bảo đảm hợp lệ thì mới lưu 
            if (upload != null && upload.ContentLength > 0)
            {
                var targetFolder = Server.MapPath("~/Uploads/Picture");
                var uniqueFileName = DateTime.Now.Ticks + "_" + upload.FileName;
                var targetFilePath = Path.Combine(targetFolder, uniqueFileName);
                upload.SaveAs(targetFilePath);
                if (!string.IsNullOrEmpty(oldfilePath))
                {
                    oldfilePath = Server.MapPath(oldfilePath);
                    System.IO.File.Delete(oldfilePath);
                }
                return Path.Combine("~/Uploads/Picture", uniqueFileName);
            }
            return oldfilePath;
        }

        private void UpdateProductCategories (Product product, int[] selectedCategories)
        {
            if (selectedCategories == null) return;
            var categories = db.Categories.ToList();
           var currentCateIds = new HashSet<int>(product.Categories.Select(x => x.CategoryId));
        
        foreach (var cate in categories)
            {
                if (selectedCategories.Contains(cate.CategoryId))
                {
                    if (!currentCateIds.Contains(cate.CategoryId))
                    {
                        product.Categories.Add(cate);
                    }
                    else if (currentCateIds.Contains(cate.CategoryId))
                        product.Categories.Remove(cate);

                }
            } 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductCreateViewModel createModel)
        {

            var discount = EnsureDiscountValue(createModel);
            var imagePath = SaveUploadFile(createModel.Upload, null);

            if (string.IsNullOrEmpty(imagePath))
                ModelState.AddModelError("Upload", "Bạn chưa chọn hình cho sản phẩm"); 
            if (createModel.SelectedCategories == null ||
                    !createModel.SelectedCategories.Any())
                ModelState.AddModelError("SelectedCategories",
                                   "Bạn phải chọn ít nhất 1 nhóm mặt hàng cho sản phẩm");
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = createModel.Name,
                    Alias = createModel.Alias,
                    ProductCode = createModel.ProductCode,
                    ShortIntro = createModel.ShortIntro,
                    Description = createModel.Description,
                    QtyPerUnit = createModel.QtyPerUnit,
                    Quantity = createModel.Quantity,
                    Price = createModel.Price,

                    SupplierId = createModel.SupplierId,
                    Actived = Convert.ToBoolean(createModel.Actived),
                    Discount = discount,
                    ThumbImage = imagePath,
                    Categories = new List<Category>()
                };
                UpdateProductCategories(product,createModel.SelectedCategories );
                var history = new ProductHistory
                {
                    AccountId = User.Identity.GetUserId(),
                    ActionTime = DateTime.UtcNow,
                    Product = product,
                    HistoryAction = ProductHistoryAction.Create,
                    OriginalProduct = null,
                    ModifiedProduct = JsonConvert.SerializeObject(product)
                };
                db.Products.Add(product);
                db.ProductHistories.Add(history);

                db.SaveChanges();
                return Redirect(product.ProductId);
            }
            InitFormData(createModel);
            return View(createModel);
        }

        // GET: Adm/Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products
                .Include(p => p.Categories)  
                .SingleOrDefault(p => p.ProductId == id);

            if (product == null) return HttpNotFound();

            var selectedCates = product.Categories.Select(x => x.CategoryId).ToArray();

            var editModel = new ProductEditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Alias = product.Alias,
                ProductCode = product.ProductCode,
                ShortIntro = product.ShortIntro,
                Description = product.Description,
                QtyPerUnit = product.QtyPerUnit,
                Quantity = product.Quantity,
                Price = product.Price,
                SupplierId = product.SupplierId,
                Actived = product.Actived ? 1 : 0,
                RowVersion = product.RowVersion,
                Discount = product.Discount,
                ThumbImage = product.ThumbImage,
                SelectedCategories = selectedCates,
            };

            InitFormData(editModel);
            return View(editModel);
        }

        // POST: Adm/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ProductEditViewModel editModel)  
        {
            if (id != editModel.ProductId)
            {
                ModelState.AddModelError("", "ID sản phẩm không khớp.");
            }

            if (!ModelState.IsValid)
            {
                InitFormData(editModel);
                return View(editModel);
            }
            var product = db.Products
                .Include(p => p.Categories)  
                .SingleOrDefault(p => p.ProductId == editModel.ProductId);

            if (product == null)
                return HttpNotFound();

            var oldprodJson = JsonConvert.SerializeObject(product);


            product.Name = editModel.Name;
            product.Alias = editModel.Alias;
            product.ProductCode = editModel.ProductCode;
            product.ShortIntro = editModel.ShortIntro;
            product.Description = editModel.Description;
            product.QtyPerUnit = editModel.QtyPerUnit;
            product.Quantity = editModel.Quantity;
            product.Price = editModel.Price;
            product.SupplierId = editModel.SupplierId;
            product.Actived = Convert.ToBoolean(editModel.Actived);
            product.Discount = EnsureDiscountValue(editModel);  
            product.ThumbImage = SaveUploadFile(editModel.Upload, product.ThumbImage);

            UpdateProductCategories(product, editModel.SelectedCategories);

            // History
            var history = new ProductHistory
            {
                AccountId = User.Identity.GetUserId(),
                ActionTime = DateTime.UtcNow,
                ProductId = product.ProductId,
                HistoryAction = ProductHistoryAction.UpdateFull,
                OriginalProduct = oldprodJson,
                ModifiedProduct = JsonConvert.SerializeObject(product),
            };
            db.ProductHistories.Add(history);

            try
            {
                db.SaveChanges();  

                return RedirectToAction("Index");  
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang và thử lại.");
                InitFormData(editModel);
                return View(editModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi lưu: {ex.Message}"); 
                InitFormData(editModel);
                return View(editModel);
            }
        }

        // GET: Adm/Product/OutOfStock
        public ActionResult OutOfStock(ProductSearchModel searchModel)
        {

            var products = db.Products
                .Include(p => p.Supplier)
                .Where(p => p.Quantity <= 0);  

            if (!string.IsNullOrWhiteSpace(searchModel.Keyword))
            {
                products = products.Where(x =>
                    x.Name.Contains(searchModel.Keyword) ||
                    x.ProductCode.Contains(searchModel.Keyword));
            }
            products = products.OrderBy(x => x.Name);
            if (!searchModel.PageSize.HasValue)
                searchModel.PageSize = 10;

            if (!searchModel.PageIndex.HasValue)
                searchModel.PageIndex = 1;

            ViewBag.PageSize = new SelectList(new[] { 5, 10, 25, 50, 100 },
                searchModel.PageSize.Value);
            ViewBag.SearchModel = searchModel;

            return View(products.ToPagedList(
                searchModel.PageIndex.Value,
                searchModel.PageSize.Value));
        }

        // POST: Adm/Product/QuickUpdateStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult QuickUpdateStock(int productId, int newQuantity)
        {
            try
            {
                var product = db.Products.Find(productId);
                if (product == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

                var oldQuantity = product.Quantity;
                product.Quantity = newQuantity;
                var history = new ProductHistory
                {
                    AccountId = User.Identity.GetUserId(),
                    ActionTime = DateTime.UtcNow,
                    ProductId = product.ProductId,
                    HistoryAction = ProductHistoryAction.UpdateQuantity,
                    Description = $"Nhập hàng: {oldQuantity} → {newQuantity}",
                    ModifiedProduct = JsonConvert.SerializeObject(new
                    {
                        productId = product.ProductId,
                        oldQuantity,
                        newQuantity,
                        product.Name
                    })
                };

                db.ProductHistories.Add(history);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Đã cập nhật số lượng thành {newQuantity}!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }
        protected override bool OnUpdateToggle(string propName, bool value, object[] keys)
        {
           
            string query = string.Format(
                "UPDATE dbo.Products SET {0} = @p0 WHERE ProductId = @p1", propName);

            return db.Database.ExecuteSqlCommand(query, value, keys[0]) > 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}