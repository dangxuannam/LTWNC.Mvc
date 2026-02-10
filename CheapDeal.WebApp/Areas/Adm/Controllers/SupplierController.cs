using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
        // GET: Adm/Supplier
        public class SupplierController : AdminController
        {
            private ShopDbContext db = new ShopDbContext();

            // GET: Adm/Supplier
            public ActionResult Index(string keyword, string actived, int? page, int? pageSize)
            {
            var suppliers = db.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                suppliers = suppliers.Where(x => x.Name.Contains(keyword) ||
                                                 x.ContactName.Contains(keyword) ||
                                                 x.Address.Contains(keyword) ||
                                                 x.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(actived))
            {
                bool isActive = actived == "1";
                suppliers = suppliers.Where(x => x.Actived == isActive);
            }

            page = page ?? 1;
            pageSize = pageSize ?? 5;

            ViewBag.Keyword = keyword;
            ViewBag.Actived = actived ?? ""; 
            ViewBag.PageSize = new SelectList(new[] { 5, 10, 25, 50, 100 }, pageSize);
            ViewBag.CurrentPageSize = pageSize;

            var data = suppliers.OrderBy(x => x.Name).ToPagedList(page.Value, pageSize.Value);

            return View(data);
        }

        // POST: Adm/Supplier/ToggleMultiple
        [HttpPost]
        public JsonResult ToggleMultiple(List<int> ids, bool actived)
        {
            if (ids == null || ids.Count == 0)
            {
                return Json(new { success = false, message = "Không có nhà cung cấp nào được chọn." });
            }

            try
            {
                int updated = 0;
                foreach (var id in ids.Distinct()) 
                {
                    var supplier = db.Suppliers.Find(id);
                    if (supplier != null)
                    {
                        supplier.Actived = actived;
                        updated++;
                    }
                }

                if (updated == 0)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhà cung cấp nào để cập nhật." });
                }

                db.SaveChanges();
                return Json(new { success = true, message = $"Đã cập nhật {updated} nhà cung cấp." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ToggleMultiple Error: " + ex.ToString());
                return Json(new { success = false, message = "Lỗi khi lưu thay đổi: " + ex.Message });
            }
        }

        // GET: Adm/Supplier/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Adm/Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection data)
        {
            var supplier = new Supplier();
            try
            {
                UpdateModel(supplier, new[]
                {
                    "Name","Description","ContactName",
                    "ContactTitle","Address","Email",
                    "Phone","Fax","HomePage","Actived"
                });

                if (ModelState.IsValid)
                {
                    db.Suppliers.Add(supplier);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(supplier);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            return View(supplier);
        }

        // POST: Adm/Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SupplierId,Name,Description,ContactName,ContactTitle,Address,Email,Phone,Fax,HomePage,Actived,RowVersion")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(supplier).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Supplier)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Không thể lưu thay đổi. Nhà cung cấp đã bị xóa bởi người dùng khác.");
                    }
                    else
                    {
                        var databaseValues = (Supplier)databaseEntry.ToObject();
                        if (databaseValues.Name != clientValues.Name)
                            ModelState.AddModelError("Name", $"Giá trị hiện tại: {databaseValues.Name}");

                        if (databaseValues.Description != clientValues.Description)
                            ModelState.AddModelError("Description", $"Giá trị hiện tại: {databaseValues.Description}");

                        if (databaseValues.ContactName != clientValues.ContactName)
                            ModelState.AddModelError("ContactName", $"Giá trị hiện tại: {databaseValues.ContactName}");


                        ModelState.AddModelError(string.Empty,
                            "Bản ghi bạn đang chỉnh sửa đã bị thay đổi bởi người dùng khác sau khi bạn tải dữ liệu. " +
                            "Thao tác chỉnh sửa bị hủy và giá trị hiện tại từ cơ sở dữ liệu đã được hiển thị. " +
                            "Nếu bạn vẫn muốn chỉnh sửa, hãy nhấn Lưu lại. Hoặc nhấn Quay lại danh sách.");

                        supplier.RowVersion = databaseValues.RowVersion;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi khi lưu: " + ex.Message);
                }
            }

            return View(supplier);
        }

        // GET: Adm/Supplier/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            return View(supplier);
        }

        // POST: Adm/Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, byte[] rowVersion)
        {
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            try
            {
                db.Entry(supplier).OriginalValues["RowVersion"] = rowVersion;
                db.Suppliers.Remove(supplier);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ConcurrencyError"] = "Nhà cung cấp đã bị thay đổi hoặc xóa bởi người dùng khác.";
                return RedirectToAction("Delete", new { id });
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
        /// <summary>
        /// Phương thức cập nhật giá trị thuộc tính Actived của lớp Supplier 
        /// </summary>
        /// <param name="propName">Thuộc tính Actived</param>
        /// <param name = "value">Giá trị mới</param>
        /// <param name = "keys">Mã nhà cung cấp </param>
        /// <returns>Trả về true nếu thành công, false nếu ngược lại </returns>

        protected override bool OnUpdateToggle(string propName, bool value, object[] keys)
        {
            try
            {
                string query = string.Format(
                    "UPDATE dbo.Suppliers SET {0} = @p0 WHERE SupplierId = @p1", propName);


                return db.Database.ExecuteSqlCommand(query, value, keys[0]) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}   