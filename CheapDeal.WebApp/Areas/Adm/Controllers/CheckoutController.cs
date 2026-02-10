using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using CheapDeal.WebApp.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Controllers
{
    /// <summary>
    /// Controller xử lý thanh toán cho khách hàng
    /// </summary>
    public class CheckoutController : Controller
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Checkout
        [Authorize]
        public ActionResult Index()
        {
            // Lấy giỏ hàng từ Session
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền
            decimal totalAmount = cart.Sum(item => item.Quantity * item.Price * (1 - item.Discount));
            ViewBag.TotalAmount = totalAmount;

            // Lấy thông tin khách hàng
            var userId = User.Identity.GetUserId();
            var customer = db.Users.Find(userId);
            ViewBag.Customer = customer;

            return View(cart);
        }

        // POST: Checkout/PlaceOrder
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string paymentMethod, string shipName, string shipAddress, string shipTel, string notes)
        {
            try
            {
                // Lấy giỏ hàng
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

                if (!cart.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra tồn kho
                foreach (var item in cart)
                {
                    var product = db.Products.Find(item.ProductId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        TempData["Error"] = $"Sản phẩm '{item.ProductName}' không đủ số lượng!";
                        return RedirectToAction("Index");
                    }
                }

                // Tạo đơn hàng
                var order = new Order
                {
                    CustomerId = User.Identity.GetUserId(),
                    OrderDate = DateTime.Now,
                    ShipName = shipName,
                    ShipAddress = shipAddress,
                    ShipTel = shipTel,
                    Notes = notes,
                    Status = OrderStatus.New,
                    Freight = 0 
                };

                // Thêm chi tiết đơn hàng
                foreach (var item in cart)
                {
                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = (int)item.Price,
                        Discount = (float)item.Discount
                    };
                    order.OrderDetails.Add(orderDetail);

                    // Trừ số lượng sản phẩm
                    var product = db.Products.Find(item.ProductId);
                    product.Quantity -= item.Quantity;
                }

                order.TotalPrice = cart.Sum(item => item.Quantity * item.Price * (1 - item.Discount));
                db.Orders.Add(order);
                db.SaveChanges();
                PaymentMethod selectedPaymentMethod = (PaymentMethod)Enum.Parse(typeof(PaymentMethod), paymentMethod);             
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = selectedPaymentMethod,
                    Amount = order.TotalPrice,
                    Status = PaymentStatus.Pending,
                    CreatedDate = DateTime.Now
                };

                db.Payments.Add(payment);
                db.SaveChanges();

                Session["Cart"] = null;

                switch (selectedPaymentMethod)
                {
                    case PaymentMethod.MoMo:
                        return RedirectToMoMo(order.OrderId, order.TotalPrice);

                    case PaymentMethod.VNPay:
                        return RedirectToVNPay(order.OrderId, order.TotalPrice);

                    case PaymentMethod.COD:
                    case PaymentMethod.BankTransfer:
                        TempData["Success"] = "Đặt hàng thành công! Mã đơn hàng: " + order.OrderId;
                        return RedirectToAction("OrderSuccess", new { id = order.OrderId });

                    default:
                        TempData["Error"] = "Phương thức thanh toán không được hỗ trợ!";
                        return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #region MoMo Payment
        private ActionResult RedirectToMoMo(int orderId, decimal amount)
        {
            try
            {
                string orderInfo = $"Thanh toán đơn hàng #{orderId}";
                string paymentUrl = PaymentService.CreateMoMoPaymentUrl(orderId, amount, orderInfo);

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi kết nối MoMo: " + ex.Message;
                return RedirectToAction("OrderSuccess", new { id = orderId });
            }
        }

        // GET: Checkout/MomoCallback
        public ActionResult MomoCallback()
        {
            try
            {
                var parameters = Request.QueryString.AllKeys.ToDictionary(k => k, k => Request.QueryString[k]);

                // Verify signature
                bool isValid = PaymentService.VerifyMoMoCallback(parameters);

                if (!isValid)
                {
                    TempData["Error"] = "Chữ ký không hợp lệ!";
                    return RedirectToAction("Index", "Home");
                }

                int orderId = int.Parse(parameters["orderId"]);
                int resultCode = int.Parse(parameters["resultCode"]);
                string transId = parameters["transId"];

                var payment = db.Payments.FirstOrDefault(p => p.OrderId == orderId);

                if (payment != null)
                {
                    payment.TransactionId = transId;
                    payment.ResponseData = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);

                    if (resultCode == 0) 
                    {
                        payment.Status = PaymentStatus.Success;
                        payment.PaidDate = DateTime.Now;

                        var order = db.Orders.Find(orderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Approved;
                        }

                        TempData["Success"] = "Thanh toán thành công!";
                    }
                    else
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.Notes = parameters.ContainsKey("message") ? parameters["message"] : "Thanh toán thất bại";

                        TempData["Error"] = "Thanh toán thất bại: " + payment.Notes;
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("OrderSuccess", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xử lý callback: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Checkout/MomoNotify (IPN)
        [HttpPost]
        public ActionResult MomoNotify()
        {
            return Json(new { resultCode = 0 });
        }

        private ActionResult RedirectToVNPay(int orderId, decimal amount)
        {
            try
            {
                string orderInfo = $"Thanh toan don hang #{orderId}";
                string ipAddress = Request.UserHostAddress;

                string paymentUrl = PaymentService.CreateVNPayPaymentUrl(orderId, amount, orderInfo, ipAddress);

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi kết nối VNPay: " + ex.Message;
                return RedirectToAction("OrderSuccess", new { id = orderId });
            }
        }
        public ActionResult VNPayCallback()
        {
            try
            {
                var parameters = Request.QueryString.AllKeys.ToDictionary(k => k, k => Request.QueryString[k]);

                // Verify signature
                bool isValid = PaymentService.VerifyVNPayCallback(parameters);

                if (!isValid)
                {
                    TempData["Error"] = "Chữ ký không hợp lệ!";
                    return RedirectToAction("Index", "Home");
                }

                int orderId = int.Parse(parameters["vnp_TxnRef"]);
                string responseCode = parameters["vnp_ResponseCode"];
                string transactionNo = parameters.ContainsKey("vnp_TransactionNo") ? parameters["vnp_TransactionNo"] : "";

                var payment = db.Payments.FirstOrDefault(p => p.OrderId == orderId);

                if (payment != null)
                {
                    payment.TransactionId = transactionNo;
                    payment.ResponseData = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);

                    if (responseCode == "00") // Thành công
                    {
                        payment.Status = PaymentStatus.Success;
                        payment.PaidDate = DateTime.Now;

                        // Cập nhật trạng thái đơn hàng
                        var order = db.Orders.Find(orderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Approved;
                        }

                        TempData["Success"] = "Thanh toán thành công!";
                    }
                    else
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.Notes = "Mã lỗi: " + responseCode;

                        TempData["Error"] = "Thanh toán thất bại!";
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("OrderSuccess", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xử lý callback: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion

        // GET: Checkout/OrderSuccess/5
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }
            var payment = db.Payments.FirstOrDefault(p => p.OrderId == id);
            ViewBag.Payment = payment;

            return View(order);
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