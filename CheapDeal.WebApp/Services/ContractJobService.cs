using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;

namespace CheapDeal.WebApp.Services
{
    public class ContractJobService
    {
        private static string GetConnStr()
        {
            var entry = ConfigurationManager.ConnectionStrings["ShopDbContext"]
                     ?? ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (entry == null)
                throw new InvalidOperationException("Không tìm thấy connection string trong Web.config");
            return entry.ConnectionString;
        }
        public static void RunDailyJobs(int daysAhead = 7)
        {
            using (var db = new ShopDbContext())
            {
                var log = new HangfireJobLog
                {
                    JobName = "daily-contract-jobs",
                    StartTime = DateTime.Now,
                    Status = "RUNNING"
                };
                try
                {
                    using (var conn = new SqlConnection(GetConnStr()))
                    {
                        conn.Open();
                        var cmd = new SqlCommand("sp_RunDailyContractJobs", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                            CommandTimeout = 120
                        };
                        cmd.Parameters.AddWithValue("@DaysAhead", daysAhead);
                        cmd.ExecuteNonQuery();

                        log.EndTime = DateTime.Now;
                        log.Status = "SUCCESS";
                        log.AffectedRows = 0;
                        log.Message = $"Daily job hoàn thành cho {daysAhead} ngày tới";
                    }
                }
                catch (Exception ex)
                {
                    log.EndTime = DateTime.Now;
                    log.Status = "FAILED";
                    log.Message = ex.Message;
                }
                db.HangfireJobLogs.Add(log);
                db.SaveChanges();
            }
        }

        public static void MarkOverdueSchedules()
        {
            using (var db = new ShopDbContext())
            {
                var log = new HangfireJobLog
                {
                    JobName = "hourly-mark-overdue",
                    StartTime = DateTime.Now,
                    Status = "RUNNING"
                };
                try
                {
                    using (var conn = new SqlConnection(GetConnStr()))
                    {
                        conn.Open();
                        var cmd = new SqlCommand("sp_MarkOverdueSchedules", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                            CommandTimeout = 60
                        };
                        cmd.ExecuteNonQuery();

                        log.EndTime = DateTime.Now;
                        log.Status = "SUCCESS";
                        log.AffectedRows = 0;
                        log.Message = "Đã cập nhật OVERDUE";
                    }
                }
                catch (Exception ex)
                {
                    log.EndTime = DateTime.Now;
                    log.Status = "FAILED";
                    log.Message = ex.Message;
                }
                db.HangfireJobLogs.Add(log);
                db.SaveChanges();
            }
        }
        public static void ScanAndCreateReminders(int daysAhead = 7)
        {
            using (var db = new ShopDbContext())
            {
                var log = new HangfireJobLog
                {
                    JobName = "scan-reminders",
                    StartTime = DateTime.Now,
                    Status = "RUNNING"
                };
                try
                {
                    using (var conn = new SqlConnection(GetConnStr()))
                    {
                        conn.Open();
                        var cmd = new SqlCommand("sp_ScanAndCreateReminders", conn)
                        {
                            CommandType = CommandType.StoredProcedure,
                            CommandTimeout = 60
                        };
                        cmd.Parameters.AddWithValue("@DaysAhead", daysAhead);
                        cmd.ExecuteNonQuery();

                        log.EndTime = DateTime.Now;
                        log.Status = "SUCCESS";
                        log.Message = $"Scan xong {daysAhead} ngày tới";
                    }
                }
                catch (Exception ex)
                {
                    log.EndTime = DateTime.Now;
                    log.Status = "FAILED";
                    log.Message = ex.Message;
                }
                db.HangfireJobLogs.Add(log);
                db.SaveChanges();
            }
        }

        public static void SendPendingEmails()
        {
            using (var db = new ShopDbContext())
            {
                var log = new HangfireJobLog
                {
                    JobName = "send-pending-emails",
                    StartTime = DateTime.Now,
                    Status = "RUNNING"
                };
                int sent = 0, failed = 0;
                try
                {
                    var pendingList = db.Reminders
                        .Where(r => r.Status == "PENDING" && r.ReminderDate <= DateTime.Now)
                        .OrderBy(r => r.ReminderDate)
                        .Take(50).ToList();

                    foreach (var reminder in pendingList)
                    {
                        try
                        {
                            var contract = db.Contracts.Include("Customer")
                                .FirstOrDefault(c => c.ContractId == reminder.ContractId);

                            var toEmail = contract?.Customer?.Email;
                            var customerName = contract?.Customer?.UserName ?? "Khách hàng";

                            if (!string.IsNullOrEmpty(toEmail))
                                GuiEmail(toEmail,
                                    reminder.Subject ?? "Nhắc nhở thanh toán",
                                    TaoNoiDungEmail(reminder, customerName));

                            reminder.Status = "SENT";
                            reminder.SentDate = DateTime.Now;
                            sent++;
                        }
                        catch (Exception) { reminder.Status = "FAILED"; failed++; }
                    }
                    log.Status = "SUCCESS";
                    log.AffectedRows = sent;
                    log.Message = $"Đã gửi: {sent}, thất bại: {failed}";
                }
                catch (Exception ex) { log.Status = "FAILED"; log.Message = ex.Message; }

                log.EndTime = DateTime.Now;
                db.HangfireJobLogs.Add(log);
                db.SaveChanges();
            }
        }

        public static void GuiEmail(string toEmail, string subject, string body)
        {
            string host = ConfigurationManager.AppSettings["Smtp.Host"] ?? "smtp.gmail.com";
            int port = int.Parse(ConfigurationManager.AppSettings["Smtp.Port"] ?? "587");
            string user = ConfigurationManager.AppSettings["Smtp.User"];
            string password = ConfigurationManager.AppSettings["Smtp.Password"];
            string from = ConfigurationManager.AppSettings["Smtp.From"] ?? user;
            string fromName = ConfigurationManager.AppSettings["Smtp.FromName"] ?? "WebTaiSan System";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Chưa cấu hình Smtp.User / Smtp.Password trong Web.config");

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(user, password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 30000;
                var mail = new MailMessage
                {
                    From = new MailAddress(from, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);
                client.Send(mail);
            }
        }

        public static bool GuiEmailTest(string toEmail, out string error)
        {
            error = null;
            try
            {
                GuiEmail(toEmail,
                    $"[Test] SMTP hoạt động - {DateTime.Now:dd/MM/yyyy HH:mm}",
                    "<h2 style='color:#1ab394'>✅ SMTP thành công!</h2><p>Email test từ WebTaiSan.</p>");
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        private static string TaoNoiDungEmail(Reminder reminder, string customerName)
        {
            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<style>
  body{{font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px}}
  .box{{max-width:600px;margin:0 auto;background:white;border-radius:8px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,.1)}}
  .hd{{background:#1ab394;padding:25px;text-align:center;color:white;font-size:22px;font-weight:bold}}
  .bd{{padding:25px;color:#333;line-height:1.7}}
  .warn{{color:#dc3545;font-weight:bold}}
  .ft{{background:#f8f9fa;padding:15px;text-align:center;color:#999;font-size:12px}}
</style></head><body><div class='box'>
  <div class='hd'>🔔 Nhắc nhở thanh toán</div>
  <div class='bd'>
    <p>Kính gửi <strong>{customerName}</strong>,</p>
    <p>{reminder.Message?.Replace(Environment.NewLine, "<br>") ?? "Bạn có kỳ thanh toán sắp đến hạn."}</p>
    <p class='warn'>⚠️ Vui lòng thanh toán đúng hạn để tránh phát sinh lãi phạt.</p>
    <p style='color:#999;font-size:12px'>Email tự động — vui lòng không reply.</p>
  </div>
  <div class='ft'>© {DateTime.Now.Year} WebTaiSan System</div>
</div></body></html>";
        }
    }
}