using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    public class PictureController : AdminController 
    {
        private ShopDbContext db = new ShopDbContext();


        // GET: Adm/Picture (hoặc Adm/Picture/Index)
        public ActionResult Index(int? productId = null)
        {
            var query = db.Picture.Include(p => p.Product).AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(p => p.ProductId == productId.Value);
            }

            var pictures = query.OrderBy(p => p.OrderNo).ThenByDescending(p => p.PictureId).ToList();
            ViewBag.SelectedProductId = productId;               
            ViewBag.QueryCount = pictures.Count;                
            ViewBag.RawQueryCount = db.Picture.Count();          
            ViewBag.HasPicturesForProduct = db.Picture.Any(p => p.ProductId == productId); // Có ảnh cho product cụ thể không

            ViewBag.Products = new SelectList(
                db.Products.OrderBy(p => p.Name).Select(p => new { p.ProductId, p.Name }),
                "ProductId",
                "Name",
                productId
            );

            return View(pictures);
        }

        // GET: Adm/Picture/Library
        public ActionResult Library(int? productId = null, int? categoryId = null)
        {
            var query = db.Picture
                          .Include(p => p.Product)
                          .Include(p => p.Product.Categories)
                          .Where(p => p.Actived);

            
            if (productId.HasValue)
            {
                query = query.Where(p => p.ProductId == productId.Value);
            }
            else if (categoryId.HasValue)  
            {
                query = query.Where(p => p.Product.Categories.Any(c => c.CategoryId == categoryId.Value));
            }

            var pictures = query.OrderByDescending(p => p.PictureId).ToList();
            ViewBag.Categories = new SelectList(db.Categories.OrderBy(c => c.Name), "CategoryID", "Name", categoryId);
            ViewBag.Products = new SelectList(db.Products.OrderBy(p => p.Name), "ProductId", "Name", productId);

            return View(pictures);
        }
        [HttpGet]
        public JsonResult GetProductsByCategory(int? categoryId)
        {
            var products = db.Products
                .Where(p => categoryId == null || p.Categories.Any(c => c.CategoryId == categoryId.Value))
                .OrderBy(p => p.Name)
                .Select(p => new { p.ProductId, p.Name })
                .ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
        }
        // GET: Adm/Picture/Upload
        public ActionResult Upload(int? productId)
        {
            ViewBag.ProductId = productId;


            ViewBag.Products = new SelectList(
                db.Products.OrderBy(p => p.Name),
                "ProductId",
                "Name",
                productId
            );

            return View();
        }

        // POST: Adm/Picture/Upload 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(Picture model, HttpPostedFileBase imageFile)
        {
            if (imageFile == null || imageFile.ContentLength <= 0)
                return Json(new { success = false, message = "Vui lòng chọn file!" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string relativePath = "/Content/Images/products/" + fileName;
                string fullPath = Server.MapPath("~" + relativePath);
                imageFile.SaveAs(fullPath);

                model.Path = relativePath;
                model.Actived = true;
                if (model.OrderNo == 0) model.OrderNo = 1;


                var product = db.Products.Find(model.ProductId);
                if (product == null)
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

                product.ThumbImage = relativePath;  

                db.Picture.Add(model);
                db.Entry(product).State = EntityState.Modified;  

                db.SaveChanges();  

                return Json(new { success = true, message = "Upload thành công!", redirectUrl = Url.Action("Library", new { productId = model.ProductId }) });
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null) msg += "\nInner: " + ex.InnerException.Message;
                return Json(new { success = false, message = "Lỗi update: " + msg });
            }
        }
        [HttpPost]
        public JsonResult DeletePicture(int id)
        {
            try
            {
                var pic = db.Picture.Find(id);
                if (pic == null) return Json(new { success = false, message = "Ảnh không tồn tại!" });

                var product = db.Products.Find(pic.ProductId);
                if (product != null && product.ThumbImage == pic.Path)
                {
                    var newThumb = db.Picture
                    .Where(p => p.ProductId == product.ProductId && p.PictureId != pic.PictureId && p.Actived)
                    .OrderBy(p => p.OrderNo)
                    .ThenByDescending(p => p.PictureId)
                    .Select(p => p.Path)
                    .FirstOrDefault();

                    product.ThumbImage = newThumb ?? "/Content/Images/products/no-image.jpg";  // fallback default
                    db.Entry(product).Property(x => x.ThumbImage).IsModified = true;
                }
                string fullPath = Server.MapPath("~" + pic.Path);
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

                db.Configuration.ValidateOnSaveEnabled = false;
                db.Picture.Remove(pic);
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;

                return Json(new { success = true, id = id }); 
            }
            catch (Exception ex)
            {

                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += "\nInner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        errorMsg += "\nDeeper Inner: " + ex.InnerException.InnerException.Message;
                }

                if (ex is DbEntityValidationException dbEx)
                {
                    errorMsg = "Validation errors:\n";
                    foreach (var eve in dbEx.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            errorMsg += $"Property: {ve.PropertyName}, Error: {ve.ErrorMessage}\n";
                        }
                    }
                }

                return Json(new { success = false, message = errorMsg });
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

