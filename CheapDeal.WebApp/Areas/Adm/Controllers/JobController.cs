using System;
using System.Linq;
using System.Web.Mvc;
using Hangfire;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Services;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    public class JobController : Controller
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: /Adm/Job/Index
        public ActionResult Index()
        {
            var logs = db.HangfireJobLogs
                .OrderByDescending(j => j.StartTime)
                .Take(20)
                .ToList();

            return View(logs);
        }

        // GET: /Adm/Job/Reminders
        public ActionResult Reminders()
        {
            var reminders = db.Reminders
                .OrderByDescending(r => r.CreatedDate)
                .Take(50)
                .ToList();

            return View(reminders);
        }

        // POST: /Adm/Job/RunNow  ← AJAX gọi vào đây
        [HttpPost]
        public JsonResult RunNow(string jobType, int daysAhead = 7)
        {
            try
            {
                switch (jobType)
                {
                    case "daily":
                        BackgroundJob.Enqueue(
                            () => ContractJobService.RunDailyJobs(daysAhead));
                        break;

                    case "overdue":
                        BackgroundJob.Enqueue(
                            () => ContractJobService.MarkOverdueSchedules());
                        break;

                    case "reminders":
                        BackgroundJob.Enqueue(
                            () => ContractJobService.ScanAndCreateReminders(daysAhead));
                        break;

                    default:
                        return Json(new { success = false, message = "Loại job không hợp lệ" });
                }

                return Json(new { success = true, message = $"Job '{jobType}' đã được đưa vào queue!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}   