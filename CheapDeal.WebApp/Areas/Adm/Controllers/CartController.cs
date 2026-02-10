using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Controllers
{
    public class CartController : Controller
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Cart
        public ActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalAmount = cart.Sum(item => item.Quantity * item.Price * (1 - item.Discount));
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var product = db.Products.Find(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
                }

                var cart = GetCart();

                var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                        Discount = (decimal)product.Discount,
                        ImageUrl = product.ThumbImage
                    });
                }

                SaveCart(cart);

                return Json(new
                {
                    success = true,
                    message = "Đã thêm vào giỏ hàng!",
                    cartCount = cart.Sum(c => c.Quantity)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(c => c.ProductId == productId);

                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        cart.Remove(item);
                    }
                    else
                    {
                        item.Quantity = quantity;
                    }

                    SaveCart(cart);

                    var totalAmount = cart.Sum(c => c.Quantity * c.Price * (1 - c.Discount));

                    return Json(new
                    {
                        success = true,
                        totalAmount = totalAmount.ToString("N0"),
                        itemTotal = (item != null ? (item.Quantity * item.Price * (1 - item.Discount)).ToString("N0") : "0")
                    });
                }

                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            try
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(c => c.ProductId == productId);

                if (item != null)
                {
                    cart.Remove(item);
                    SaveCart(cart);

                    return Json(new
                    {
                        success = true,
                        message = "Đã xóa sản phẩm!",
                        cartCount = cart.Sum(c => c.Quantity)
                    });
                }

                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Cart/ClearCart
        [HttpPost]
        public ActionResult ClearCart()
        {
            Session["Cart"] = null;
            return Json(new { success = true, message = "Đã xóa toàn bộ giỏ hàng!" });
        }
        private List<CartItem> GetCart()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }
        private void SaveCart(List<CartItem> cart)
        {
            Session["Cart"] = cart;
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