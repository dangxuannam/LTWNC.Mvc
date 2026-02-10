using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Message
        public ActionResult Index(string type = "inbox")
        {
            var userId = User.Identity.GetUserId();
            IQueryable<Message> messages; 

            if (type == "sent")
            {
                messages = db.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Where(m => m.SenderId == userId && !m.IsDeletedBySender)
                    .OrderByDescending(m => m.SentDate);
            }
            else
            {
                messages = db.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Where(m => m.ReceiverId == userId && !m.IsDeletedByReceiver)
                    .OrderByDescending(m => m.SentDate);
            }

            ViewBag.Type = type;
            ViewBag.UnreadCount = db.Messages
                .Count(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeletedByReceiver);

            return View(messages.ToList());
        }
        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var message = db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefault(m => m.MessageId == id);

            if (message == null)
            {
                return HttpNotFound();
            }
            if (message.SenderId != userId && message.ReceiverId != userId)
            {
                return new HttpUnauthorizedResult();
            }

            if (message.ReceiverId == userId && !message.IsRead)
            {
                message.IsRead = true;
                message.ReadDate = DateTime.Now;
                db.Entry(message).State = EntityState.Modified;
                db.SaveChanges();
            }

            return View(message);
        }

        // GET: Message/Compose
        public ActionResult Compose(string to = null)
        {
            ViewBag.Users = new SelectList(
                db.Users.Where(u => u.Id != User.Identity.GetUserId()).OrderBy(u => u.UserName),
                "Id", "UserName", to);

            return View();
        }

        // POST: Message/Compose
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Compose(Message model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                model.SenderId = userId;
                model.SentDate = DateTime.Now;
                model.IsRead = false;

                db.Messages.Add(model);
                db.SaveChanges();

                var notification = new AllModels.Notification
                {
                    AccountId = model.ReceiverId,
                    Title = "Tin nhắn mới",
                    Content = $"Bạn có tin nhắn mới từ {User.Identity.GetUserName()}: {model.Subject}",
                    Type = "Message",
                    Link = "/Message/Details/" + model.MessageId,
                    CreatedDate = DateTime.Now,  
                    IsRead = false               
                };

                db.Notifications.Add(notification);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Gửi tin nhắn thành công!";
                return RedirectToAction("Index", new { type = "sent" });
            }

            ViewBag.Users = new SelectList(
                db.Users.Where(u => u.Id != User.Identity.GetUserId()).OrderBy(u => u.UserName),
                "Id", "UserName", model.ReceiverId);

            return View(model);
        }

        // GET: Message/Reply/5
        public ActionResult Reply(int id)
        {
            var message = db.Messages
                .Include(m => m.Sender)
                .FirstOrDefault(m => m.MessageId == id);

            if (message == null)
            {
                return HttpNotFound();
            }

            var reply = new Message 
            {
                ReceiverId = message.SenderId,
                Subject = "RE: " + message.Subject
            };

            ViewBag.OriginalMessage = message;
            ViewBag.Users = new SelectList(
                db.Users.Where(u => u.Id != User.Identity.GetUserId()).OrderBy(u => u.UserName),
                "Id", "UserName", message.SenderId);

            return View("Compose", reply);
        }

        // POST: Message/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string type)
        {
            var userId = User.Identity.GetUserId();
            var message = db.Messages.Find(id);

            if (message == null)
            {
                return HttpNotFound();
            }

            // Đánh dấu xóa
            if (message.SenderId == userId)
            {
                message.IsDeletedBySender = true;
            }
            else if (message.ReceiverId == userId)
            {
                message.IsDeletedByReceiver = true;
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
            if (message.IsDeletedBySender && message.IsDeletedByReceiver)
            {
                db.Messages.Remove(message);
            }
            else
            {
                db.Entry(message).State = EntityState.Modified;
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Xóa tin nhắn thành công!";

            return RedirectToAction("Index", new { type = type ?? "inbox" });
        }

        // GET: Message/GetUnreadCount
        public JsonResult GetUnreadCount()
        {
            var userId = User.Identity.GetUserId();
            var count = db.Messages
                .Count(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeletedByReceiver);

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
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