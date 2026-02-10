using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    [Authorize(Roles = "Admin,Manager,Salesman")]
    public class OrderController : AdminController
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Adm/Order
        public ActionResult Index(string searchString, OrderStatus? status, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 20;

            var orders = db.Orders
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o =>
                    o.ShipName.Contains(searchString) ||
                    o.ShipTel.Contains(searchString) ||
                    o.OrderId.ToString().Contains(searchString));
            }
            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == status.Value);
            }
            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= toDate.Value);
            }

            orders = orders.OrderByDescending(o => o.OrderDate);

            var totalOrders = orders.Count();
            var orderList = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.TotalOrders = totalOrders;

            return View(orderList);
        }

        // GET: Adm/Order/Unship
        public ActionResult Unship(string searchString, int page = 1)
        {
            int pageSize = 20;

            var orders = db.Orders
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Approved)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o =>
                    o.ShipName.Contains(searchString) ||
                    o.ShipTel.Contains(searchString) ||
                    o.OrderId.ToString().Contains(searchString));
            }

            var totalOrders = orders.Count();
            var orderList = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.TotalOrders = totalOrders;

            return View(orderList);
        }

        // GET: Adm/Order/Shipping
        public ActionResult Shipping(string searchString, int page = 1)
        {
            int pageSize = 20;

            var orders = db.Orders
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == OrderStatus.Shipping)
                .OrderByDescending(o => o.ShipDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o =>
                    o.ShipName.Contains(searchString) ||
                    o.ShipTel.Contains(searchString) ||
                    o.OrderId.ToString().Contains(searchString));
            }

            var totalOrders = orders.Count();
            var orderList = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.TotalOrders = totalOrders;

            return View(orderList);
        }

        // GET: Adm/Order/Overview
        public ActionResult Overview(int? year, int? month)
        {

            int selectedYear = year ?? DateTime.Now.Year;
            int? selectedMonth = month;
            var orders = db.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Where(o => o.Status == OrderStatus.Success && o.OrderDate.HasValue)
                .AsQueryable();
            orders = orders.Where(o => o.OrderDate.Value.Year == selectedYear);
            if (selectedMonth.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Value.Month == selectedMonth.Value);
            }

            var orderList = orders.ToList();
            decimal totalRevenue = orderList.Sum(o => o.TotalPrice);
            int totalOrders = orderList.Count();
            decimal avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            int totalProductsSold = orderList.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity);

            List<decimal> revenueByTime;
            List<string> timeLabels;

            if (selectedMonth.HasValue)
            {
                int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth.Value);
                revenueByTime = new List<decimal>();
                timeLabels = new List<string>();

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var dayRevenue = orderList
                        .Where(o => o.OrderDate.Value.Day == day)
                        .Sum(o => o.TotalPrice);

                    revenueByTime.Add(dayRevenue);
                    timeLabels.Add($"Ngày {day}");
                }
            }
            else
            {
                revenueByTime = new List<decimal>();
                timeLabels = new List<string>();

                for (int m = 1; m <= 12; m++)
                {
                    var monthRevenue = orderList
                        .Where(o => o.OrderDate.Value.Month == m)
                        .Sum(o => o.TotalPrice);

                    revenueByTime.Add(monthRevenue);
                    timeLabels.Add($"Tháng {m}");
                }
            }

            var topProducts = orderList
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => new { od.ProductId, od.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.Price * (1 - od.Discount / 100))
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(10)
                .ToList();
            var earliestYear = db.Orders
                .Where(o => o.OrderDate.HasValue)
                .OrderBy(o => o.OrderDate)
                .Select(o => o.OrderDate.Value.Year)
                .FirstOrDefault();

            if (earliestYear == 0)
            {
                earliestYear = DateTime.Now.Year;
            }

            var years = new List<int>();
            for (int y = earliestYear; y <= DateTime.Now.Year; y++)
            {
                years.Add(y);
            }

            ViewBag.Years = years;
            ViewBag.Months = Enumerable.Range(1, 12).ToList();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.AvgOrderValue = avgOrderValue;
            ViewBag.TotalProductsSold = totalProductsSold;
            ViewBag.RevenueByTime = revenueByTime;
            ViewBag.TimeLabels = timeLabels;
            ViewBag.TopProducts = topProducts;

            return View();
        }

        // GET: Adm/Order/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Adm/Order/Create
        public ActionResult Create()
        {
            PrepareViewBagData();
            return View(new Order());
        }

        // POST: Adm/Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Order order, int[] productIds, int[] quantities, decimal[] prices, float[] discounts)
        {
            try
            {
                ModelState.Remove("CustomerId");
                ModelState.Remove("EmployeeId");
                ModelState.Remove("OrderDate");
                ModelState.Remove("Status");
                ModelState.Remove("TotalPrice");
                ModelState.Remove("OrderDetails");

                if (string.IsNullOrEmpty(order.CustomerId))
                {
                    order.CustomerId = User.Identity.GetUserId();
                }
                if (!ModelState.IsValid)
                {
                    var allErrors = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            foreach (var error in state.Errors)
                            {
                                allErrors.Add($"{key}: {error.ErrorMessage}");
                            }
                        }
                    }
                    TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join("; ", allErrors);
                    PrepareViewBagData(order);
                    return View(order);
                }

                if (productIds == null || productIds.Length == 0)
                {
                    TempData["Error"] = "Vui lòng thêm ít nhất một sản phẩm vào đơn hàng!";
                    PrepareViewBagData(order);
                    return View(order);
                }

                order.EmployeeId = User.Identity.GetUserId();
                order.OrderDate = DateTime.Now;
                order.Status = OrderStatus.New;

                decimal totalAmount = 0;

                db.Orders.Add(order);
                db.SaveChanges(); 


                for (int i = 0; i < productIds.Length; i++)
                {
                    var product = db.Products.Find(productIds[i]);
                    if (product != null && product.Quantity >= quantities[i])
                    {
                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderId,  
                            ProductId = productIds[i],
                            Quantity = quantities[i],
                            Price = (int)prices[i],
                            Discount = discounts[i]  
                        };

                        db.OrderDetails.Add(detail); 

                        product.Quantity -= quantities[i];
                        db.Entry(product).State = EntityState.Modified;


                        totalAmount += prices[i] * quantities[i] * (1 - (decimal)discounts[i] / 100);
                    }
                }

                order.TotalPrice = totalAmount + order.Freight;
                db.Entry(order).State = EntityState.Modified;

                db.SaveChanges();

                TempData["Success"] = "Tạo đơn hàng thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                var fullMessage = ex.Message + (string.IsNullOrEmpty(innerMessage) ? "" : " | Inner: " + innerMessage);

                if (ex.InnerException?.InnerException != null)
                {
                    fullMessage += " | Inner2: " + ex.InnerException.InnerException.Message;
                }

                TempData["Error"] = "Có lỗi xảy ra: " + fullMessage;
                PrepareViewBagData(order);
                return View(order);
            }
        }

        // GET: Adm/Order/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.Status != OrderStatus.New && order.Status != OrderStatus.Approved)
            {
                TempData["Error"] = "Chỉ có thể chỉnh sửa đơn hàng ở trạng thái 'Mới' hoặc 'Đã xác nhận'!";
                return RedirectToAction("Index");
            }

            PrepareViewBagData(order);
            return View(order);
        }

        // POST: Adm/Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order, int[] productIds, int[] quantities, decimal[] prices, float[] discounts)
        {
            try
            {
                ModelState.Remove("CustomerId");
                ModelState.Remove("EmployeeId");
                ModelState.Remove("OrderDate");
                ModelState.Remove("Status");
                ModelState.Remove("TotalPrice");
                ModelState.Remove("OrderDetails");

                if (ModelState.IsValid)
                {
                    var existingOrder = db.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefault(o => o.OrderId == order.OrderId);

                    if (existingOrder == null)
                    {
                        return HttpNotFound();
                    }

                    foreach (var detail in existingOrder.OrderDetails.ToList())
                    {
                        var product = db.Products.Find(detail.ProductId);
                        if (product != null)
                        {
                            product.Quantity += detail.Quantity;
                            db.Entry(product).State = EntityState.Modified;
                        }
                        db.OrderDetails.Remove(detail);
                    }

                    existingOrder.ShipName = order.ShipName;
                    existingOrder.ShipTel = order.ShipTel;
                    existingOrder.ShipAddress = order.ShipAddress;
                    existingOrder.RequiredDate = order.RequiredDate;
                    existingOrder.ShipVia = order.ShipVia;
                    existingOrder.Freight = order.Freight;
                    existingOrder.Notes = order.Notes;

                    decimal totalAmount = 0;
                    if (productIds != null && productIds.Length > 0)
                    {
                        for (int i = 0; i < productIds.Length; i++)
                        {
                            var product = db.Products.Find(productIds[i]);
                            if (product != null && product.Quantity >= quantities[i])
                            {
                                var detail = new OrderDetail
                                {
                                    OrderId = existingOrder.OrderId,
                                    ProductId = productIds[i],
                                    Quantity = quantities[i],
                                    Price = (int)prices[i],
                                    Discount = discounts[i]  
                                };

                                db.OrderDetails.Add(detail);
                                product.Quantity -= quantities[i];
                                db.Entry(product).State = EntityState.Modified;
                                totalAmount += prices[i] * quantities[i] * (1 - (decimal)discounts[i] / 100);
                            }
                        }
                    }                  
                    existingOrder.TotalPrice = totalAmount + existingOrder.Freight;

                    db.Entry(existingOrder).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Success"] = "Cập nhật đơn hàng thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var allErrors = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            foreach (var error in state.Errors)
                            {
                                allErrors.Add($"{key}: {error.ErrorMessage}");
                            }
                        }
                    }
                    TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join("; ", allErrors);
                }

                PrepareViewBagData(order);
                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                PrepareViewBagData(order);
                return View(order);
            }
        }

        // POST: Adm/Order/UpdateStatus
        [HttpPost]
        public JsonResult UpdateStatus(int orderId, OrderStatus newStatus, int? shipperId = null)
        {
            try
            {
                var order = db.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.OrderId == orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }
                if (!IsValidStatusTransition(order.Status, newStatus))
                {
                    return Json(new { success = false, message = "Không thể chuyển sang trạng thái này" });
                }

                order.Status = newStatus;
                if (newStatus == OrderStatus.Shipping)
                {
                    if (!shipperId.HasValue)
                    {
                        return Json(new { success = false, message = "Vui lòng chọn đơn vị vận chuyển" });
                    }
                    order.ShipVia = shipperId.Value;
                    order.ShipDate = DateTime.Now;
                }
                else if (newStatus == OrderStatus.Success)
                {
                    
                }
                else if (newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Returned)
                {
                    var orderDetails = order.OrderDetails.ToList();
                    foreach (var detail in orderDetails)
                    {
                        var product = db.Products.Find(detail.ProductId);
                        if (product != null)
                        {
                            product.Quantity += detail.Quantity;
                            db.Entry(product).State = EntityState.Modified;
                        }
                    }
                }

                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Adm/Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var order = db.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.OrderId == id);

                if (order == null)
                {
                    return HttpNotFound();
                }
                if (order.Status != OrderStatus.New && order.Status != OrderStatus.Cancelled)
                {
                    TempData["Error"] = "Chỉ có thể xóa đơn hàng ở trạng thái 'Mới' hoặc 'Đã hủy'!";
                    return RedirectToAction("Index");
                }
                foreach (var detail in order.OrderDetails.ToList())
                {
                    var product = db.Products.Find(detail.ProductId);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity;
                        db.Entry(product).State = EntityState.Modified;
                    }
                }

                db.Orders.Remove(order);
                db.SaveChanges();

                TempData["Success"] = "Xóa đơn hàng thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Adm/Order/Print/5
        public ActionResult Print(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        private void PrepareViewBagData(Order order = null)
        {
            try
            {
                var allRates = db.ShippingRates
                    .Include(r => r.Shipper)
                    .ToList();

                ViewBag.ShippingRates = allRates
                    .Select(r => new
                    {
                        Id = r.Id,
                        DisplayText = $"{r.Shipper.CompanyName} - {(string.IsNullOrEmpty(r.ProvinceName) ? "Toàn quốc" : r.ProvinceName)} ({r.MinWeight}-{r.MaxWeight}kg)",
                        Price = r.Price,
                        ShipperId = r.ShipperId
                    })
                    .ToList();
                var allProducts = db.Products.Where(p => p.Actived).ToList();
                ViewBag.Products = allProducts
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Name,
                        p.Price,
                        p.Discount,
                        p.Quantity
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PrepareViewBagData: {ex.Message}");
                ViewBag.ShippingRates = new List<object>();
                ViewBag.Products = new List<object>();
            }
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            switch (currentStatus)
            {
                case OrderStatus.New:
                    return newStatus == OrderStatus.Approved ||
                           newStatus == OrderStatus.Cancelled;

                case OrderStatus.Approved:
                    return newStatus == OrderStatus.Shipping ||
                           newStatus == OrderStatus.Cancelled;

                case OrderStatus.Shipping:
                    return newStatus == OrderStatus.Success ||
                           newStatus == OrderStatus.Returned;

                case OrderStatus.Success:
                    return newStatus == OrderStatus.Returned;

                default:
                    return false;
            }
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