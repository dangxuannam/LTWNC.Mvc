using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    public class ShipperController : AdminController
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Adm/Shipper
        public ActionResult Index()
        {
            return View(db.Shippers.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // POST: Adm/Shipper/Delete/5
        [HttpPost]
        public JsonResult Delete(int id, bool force = false)
        {
            try
            {
                Shipper shipper = db.Shippers.Find(id);
                if (shipper == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy công ty vận chuyển." });
                }

                var orders = db.Orders.Where(o => o.ShipVia == id).ToList();
                if (orders.Any() && !force)
                {
                    return Json(new { success = false, hasOrders = true, message = "Công ty này đang có đơn hàng được giao. Không thể xóa." });
                }
                else
                {
                    if (orders.Any())
                    {
                        foreach (var order in orders)
                        {
                            order.ShipVia = null;
                            db.Entry(order).State = EntityState.Modified;
                        }
                    }
                    db.Shippers.Remove(shipper);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Adm/Shipper/DeleteMultiple
        [HttpPost]
        public JsonResult DeleteMultiple(List<int> ids, bool force = false)
        {
            try
            {
                bool hasAnyOrders = false;
                foreach (int id in ids)
                {
                    Shipper shipper = db.Shippers.Find(id);
                    if (shipper == null) continue;

                    var orders = db.Orders.Where(o => o.ShipVia == id).ToList();
                    if (orders.Any())
                    {
                        hasAnyOrders = true;
                        if (!force)
                        {
                            continue;
                        }
                        foreach (var order in orders)
                        {
                            order.ShipVia = null;
                            db.Entry(order).State = EntityState.Modified;
                        }
                    }
                    db.Shippers.Remove(shipper);
                }

                if (hasAnyOrders && !force)
                {
                    return Json(new { success = false, hasOrders = true, message = "Một số công ty đang có đơn hàng được giao. Không thể xóa." });
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Adm/Shipper/SetInTransit/5
        [HttpPost]
        public JsonResult SetInTransit(int id)
        {
            try
            {
                var orders = db.Orders.Where(o => o.ShipVia == id && o.OrderDate == null).ToList();
                if (!orders.Any())
                {
                    return Json(new { success = false, message = "Không có đơn hàng nào chưa vận chuyển để cập nhật trạng thái." });
                }

                foreach (var order in orders)
                {
                    order.OrderDate = DateTime.Now;
                    db.Entry(order).State = EntityState.Modified;
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Adm/Shipper/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Adm/Shipper/Create 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CompanyName,Phone,Address")] Shipper shipper)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Shippers.Add(shipper);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi trong thêm mới. Error: " + ex.Message);
            }

            return View(shipper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ShipperID,CompanyName,Phone,Address")] Shipper shipper)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(shipper).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi trong thêm mới. Error: " + ex.Message);
            }

            return View(shipper);
        }

        // GET: Adm/Shipper/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Shipper shipper = db.Shippers.Find(id);
            if (shipper == null)
            {
                return HttpNotFound();
            }
            return View(shipper);
        }

        public ActionResult Rates(int? shipperId)
        {
            ViewBag.Shippers = new SelectList(db.Shippers.ToList(), "ShipperID", "CompanyName", shipperId);
            var rates = db.ShippingRates.Include(s => s.Shipper).AsQueryable();
            if (shipperId.HasValue)
            {
                rates = rates.Where(x => x.ShipperId == shipperId.Value);
            }

            return View(rates.OrderBy(x => x.Shipper.CompanyName).ThenBy(x => x.Price).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRate(ShippingRate model)
        {
            if (ModelState.IsValid)
            {
                db.ShippingRates.Add(model);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ anh ơi!" });
        }

        [HttpPost]
        public JsonResult DeleteRate(int id)
        {
            try
            {
                var rate = db.ShippingRates.Find(id);
                if (rate == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy mức giá này." });
                }

                db.ShippingRates.Remove(rate);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GetRateDetails(int id)
        {
            try
            {
                var rate = db.ShippingRates.Find(id);
                if (rate == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy mức giá này." });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = rate.Id,  
                        shipperId = rate.ShipperId,
                        provinceName = rate.ProvinceName,
                        minWeight = rate.MinWeight,
                        maxWeight = rate.MaxWeight,
                        price = rate.Price
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditRate(ShippingRate model)
        {
            if (ModelState.IsValid)
            {
                var existing = db.ShippingRates.Find(model.Id);  
                if (existing == null)
                    return Json(new { success = false, message = "Không tìm thấy mức giá để cập nhật" });

                existing.ShipperId = model.ShipperId;
                existing.ProvinceName = model.ProvinceName;
                existing.MinWeight = model.MinWeight;
                existing.MaxWeight = model.MaxWeight;
                existing.Price = model.Price;

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors) });
        }

        public ActionResult TrackOrders(int? shipperId, string status, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Shippers = new SelectList(db.Shippers.ToList(), "ShipperID", "CompanyName", shipperId);

            var orders = db.Orders.Include(o => o.Shipper).AsQueryable();

            if (shipperId.HasValue)
            {
                orders = orders.Where(o => o.ShipVia == shipperId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        orders = orders.Where(o => o.OrderDate == null);
                        break;
                    case "intransit":
                        orders = orders.Where(o => o.OrderDate != null);
                        break;
                    case "delivered":
                        orders = orders.Where(o => o.OrderDate != null);
                        break;
                }
            }

            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= toDate.Value);
            }

            var result = orders.Select(o => new OrderTrackingViewModel
            {
                OrderID = o.OrderId,
                CustomerID = o.CustomerId.ToString(),
                ShipperName = o.Shipper != null ? o.Shipper.CompanyName : "N/A",
                OrderDate = o.OrderDate,
                Freight = o.Freight,
                Status = o.OrderDate != null ? "Đang vận chuyển" : "Chờ xử lý"
            }).ToList();

            return View(result);
        }

        [HttpPost]
        public JsonResult UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                var order = db.Orders.Find(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                switch (newStatus.ToLower())
                {
                    case "intransit":
                        order.OrderDate = DateTime.Now;
                        break;
                    case "delivered":
                        if (order.OrderDate == null)
                        {
                            order.OrderDate = DateTime.Now;
                        }
                        break;
                    case "pending":
                        order.OrderDate = null;
                        break;
                }

                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public ActionResult PerformanceReport(int? shipperId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue)
            {
                fromDate = DateTime.Now.AddDays(-30);
            }
            if (!toDate.HasValue)
            {
                toDate = DateTime.Now;
            }

            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Shippers = new SelectList(db.Shippers.ToList(), "ShipperID", "CompanyName", shipperId);

            var query = db.Orders
                .Include(o => o.Shipper)
                .Where(o => o.OrderDate >= fromDate.Value && o.OrderDate <= toDate.Value);

            if (shipperId.HasValue)
            {
                query = query.Where(o => o.ShipVia == shipperId.Value);
            }
            var performanceData = query
                .GroupBy(o => new { o.ShipVia, o.Shipper.CompanyName })
                .Select(g => new
                {
                    ShipVia = g.Key.ShipVia,
                    CompanyName = g.Key.CompanyName,
                    TotalOrders = g.Count(),
                    PendingOrders = g.Count(o => o.OrderDate == null),
                    InTransitOrders = g.Count(o => o.OrderDate != null),
                    DeliveredOrders = g.Count(o => o.OrderDate != null),
                    TotalRevenue = g.Sum(o => o.Freight)
                })
                .ToList()
                .Select(g => new ShipperPerformanceViewModel
                {
                    ShipperID = g.ShipVia.HasValue ? g.ShipVia.Value : 0,
                    ShipperName = g.CompanyName,
                    TotalOrders = g.TotalOrders,
                    PendingOrders = g.PendingOrders,
                    InTransitOrders = g.InTransitOrders,
                    DeliveredOrders = g.DeliveredOrders,
                    TotalRevenue = g.TotalRevenue,
                    AverageDeliveryTime = 0
                })
                .OrderByDescending(p => p.TotalOrders)
                .ToList();

            foreach (var item in performanceData)
            {
                if (item.TotalOrders > 0)
                {
                    item.DeliveryRate = (double)item.DeliveredOrders / item.TotalOrders * 100;
                    item.OnTimeRate = 95;
                }
            }

            return View(performanceData);
        }

        public JsonResult GetPerformanceChart(int? shipperId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue)
            {
                fromDate = DateTime.Now.AddDays(-30);
            }
            if (!toDate.HasValue)
            {
                toDate = DateTime.Now;
            }

            var query = db.Orders
                .Include(o => o.Shipper)
                .Where(o => o.OrderDate >= fromDate.Value && o.OrderDate <= toDate.Value);

            if (shipperId.HasValue)
            {
                query = query.Where(o => o.ShipVia == shipperId.Value);
            }
            var chartData = query
                .GroupBy(o => new { o.ShipVia, o.Shipper.CompanyName })
                .Select(g => new
                {
                    shipperName = g.Key.CompanyName,
                    totalOrders = g.Count(),
                    deliveredOrders = g.Count(o => o.OrderDate != null),
                    revenue = g.Sum(o => o.Freight)
                })
                .ToList();

            return Json(chartData, JsonRequestBehavior.AllowGet);
        }
    }

    public class OrderTrackingViewModel
    {
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public string ShipperName { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal Freight { get; set; }
        public string Status { get; set; }
    }

    public class ShipperPerformanceViewModel
    {
        public int ShipperID { get; set; }
        public string ShipperName { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int InTransitOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double DeliveryRate { get; set; }
        public double OnTimeRate { get; set; }
    }
}