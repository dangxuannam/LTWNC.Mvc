using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Hangfire;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Services;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class NotificationController : Controller
    {
        private readonly ShopDbContext _db = new ShopDbContext();

        // GET: /Adm/Notification/Index
        public ActionResult Index()
        {
            ViewBag.PendingCount = _db.Reminders.Count(r =>
                r.Status == "PENDING" && r.ReminderDate <= DateTime.Now);

            ViewBag.SentToday = _db.Reminders.Count(r =>
                r.Status == "SENT" &&
                r.SentDate.HasValue &&
                DbFunctions.TruncateTime(r.SentDate.Value) ==
                DbFunctions.TruncateTime(DateTime.Today));

            ViewBag.FailedCount = _db.Reminders.Count(r => r.Status == "FAILED");
            ViewBag.TotalSent = _db.Reminders.Count(r => r.Status == "SENT");

            var recent = _db.Reminders
                .Include("Contract")
                .Include("Contract.Customer")
                .Where(r => r.Status == "SENT" || r.Status == "FAILED")
                .OrderByDescending(r => r.SentDate ?? r.CreatedDate)
                .Take(10)
                .ToList();

            return View(recent);
        }

        // POST: Gửi ngay tất cả PENDING
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendNow()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue(
                    () => ContractJobService.SendPendingEmails());

                return Json(new
                {
                    success = true,
                    message = $"Đã queue job — Hangfire ID: {jobId}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Test SMTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendTest(string testEmail)
        {
            if (string.IsNullOrEmpty(testEmail))
                return Json(new { success = false, message = "Vui lòng nhập email" });

            string error;
            bool ok = ContractJobService.GuiEmailTest(testEmail, out error);

            return Json(new
            {
                success = ok,
                message = ok ? $"Đã gửi test đến {testEmail}" : $"Lỗi: {error}"
            });
        }

        // POST: Retry reminder FAILED
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Retry(int reminderId)
        {
            var reminder = _db.Reminders.Find(reminderId);
            if (reminder == null)
                return Json(new { success = false, message = "Không tìm thấy" });

            reminder.Status = "PENDING";
            reminder.SentDate = null;
            reminder.ReminderDate = DateTime.Now;
            _db.SaveChanges();

            return Json(new { success = true, message = "Đã reset về PENDING" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}