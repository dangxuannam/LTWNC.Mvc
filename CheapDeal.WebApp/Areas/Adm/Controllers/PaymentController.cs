using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Newtonsoft.Json;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{

    public class PaymentController : Controller
    {
        public ActionResult Payment()
        {
            var model = new PaymentGatewayViewModel
            {
                // MOMO
                EnableMomo = true,
                MomoPartnerCode = "MOMOXXXX",
                MomoAccessKey = "your_access_key",
                MomoSecretKey = "your_secret_key",
                MomoReturnUrl = "https://yourdomain.com/payment/momo/callback",
                MomoNotifyUrl = "https://yourdomain.com/payment/momo/notify",

                // VNPAY
                EnableVNPay = true,
                VNPayTmnCode = "VNPAYXXXX",
                VNPayHashSecret = "your_hash_secret",
                VNPayUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
                VNPayReturnUrl = "https://yourdomain.com/payment/vnpay/callback",

                // BANK TRANSFER
                EnableBankTransfer = true,
                BankName = "Vietcombank",
                BankBranch = "Chi nhánh Hà Nội",
                BankAccountNumber = "1234567890",
                BankAccountName = "CONG TY CHEAPDEAL",
                BankSwiftCode = "BFTVVNVX",

                // COD
                EnableCOD = true,
                CODFee = 15000,
                CODMaxAmount = 5000000,
                CODNote = "Thu tiền mặt khi giao hàng",

                // ZALOPAY
                EnableZaloPay = false,
                ZaloPayAppId = "",
                ZaloPayKey1 = "",
                ZaloPayKey2 = "",

                // PAYPAL
                EnablePayPal = false,
                PayPalClientId = "",
                PayPalClientSecret = "",
                PayPalMode = "sandbox"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(PaymentGatewayViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SaveSetting("EnableMomo", model.EnableMomo.ToString());
                    SaveSetting("MomoPartnerCode", model.MomoPartnerCode);
                    SaveSetting("MomoAccessKey", model.MomoAccessKey);
                    SaveSetting("MomoSecretKey", model.MomoSecretKey);
                    SaveSetting("MomoReturnUrl", model.MomoReturnUrl);
                    SaveSetting("MomoNotifyUrl", model.MomoNotifyUrl);

                    SaveSetting("EnableVNPay", model.EnableVNPay.ToString());
                    SaveSetting("VNPayTmnCode", model.VNPayTmnCode);
                    SaveSetting("VNPayHashSecret", model.VNPayHashSecret);
                    SaveSetting("VNPayUrl", model.VNPayUrl);
                    SaveSetting("VNPayReturnUrl", model.VNPayReturnUrl);

                    SaveSetting("EnableBankTransfer", model.EnableBankTransfer.ToString());
                    SaveSetting("BankName", model.BankName);
                    SaveSetting("BankBranch", model.BankBranch);
                    SaveSetting("BankAccountNumber", model.BankAccountNumber);
                    SaveSetting("BankAccountName", model.BankAccountName);
                    SaveSetting("BankSwiftCode", model.BankSwiftCode);

                    //  COD
                    SaveSetting("EnableCOD", model.EnableCOD.ToString());
                    SaveSetting("CODFee", model.CODFee.ToString());
                    SaveSetting("CODMaxAmount", model.CODMaxAmount.ToString());
                    SaveSetting("CODNote", model.CODNote);

                    //  ZaloPay
                    SaveSetting("EnableZaloPay", model.EnableZaloPay.ToString());
                    SaveSetting("ZaloPayAppId", model.ZaloPayAppId);
                    SaveSetting("ZaloPayKey1", model.ZaloPayKey1);
                    SaveSetting("ZaloPayKey2", model.ZaloPayKey2);

                    // PayPal
                    SaveSetting("EnablePayPal", model.EnablePayPal.ToString());
                    SaveSetting("PayPalClientId", model.PayPalClientId);
                    SaveSetting("PayPalClientSecret", model.PayPalClientSecret);
                    SaveSetting("PayPalMode", model.PayPalMode);

                    TempData["Success"] = "Cập nhật cài đặt cổng thanh toán thành công!";
                    return RedirectToAction("Payment");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return View(model);
        }      
        public ActionResult History(string search, string status, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 20;
            var query = GetPaymentTransactions();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.OrderCode.Contains(search) ||
                    p.CustomerName.Contains(search) ||
                    p.TransactionId.Contains(search)
                ).ToList();
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status).ToList();
            }
            if (fromDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= fromDate.Value).ToList();
            }
            if (toDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= toDate.Value).ToList();
            }
            var model = new PaymentHistoryViewModel
            {
                Transactions = query.OrderByDescending(p => p.PaymentDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList(),
                TotalTransactions = query.Count,
                TotalAmount = query.Sum(p => p.Amount),
                SuccessAmount = query.Where(p => p.Status == "Success").Sum(p => p.Amount),
                PendingAmount = query.Where(p => p.Status == "Pending").Sum(p => p.Amount),
                FailedAmount = query.Where(p => p.Status == "Failed").Sum(p => p.Amount),
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)query.Count / pageSize),
                SearchKeyword = search,
                SelectedStatus = status,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ExportHistory(string status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = GetPaymentTransactions();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status).ToList();
                }
                if (fromDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate <= toDate.Value).ToList();
                }

                TempData["Success"] = "Export thành công!";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi export: " + ex.Message;
                return RedirectToAction("History");
            }
        }

        public ActionResult PaymentDetail(int id)
        {
            var transaction = GetPaymentTransactions().FirstOrDefault(p => p.Id == id);
            if (transaction == null)
            {
                TempData["Error"] = "Không tìm thấy giao dịch!";
                return RedirectToAction("History");
            }

            return View(transaction);
        }
        public ActionResult Report(string period = "month", int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;

            var model = new FinancialReportViewModel
            {
                Period = period,
                Year = year,
                Month = month,             
                TotalRevenue = CalculateTotalRevenue(period, year, month),
                TotalOrders = CalculateTotalOrders(period, year, month),
                AverageOrderValue = CalculateAverageOrderValue(period, year, month),
                PaymentMethodStats = GetPaymentMethodStats(period, year, month),
                StatusStats = GetStatusStats(period, year, month),
                RevenueChartData = GetRevenueChartData(period, year, month),
                OrderChartData = GetOrderChartData(period, year, month),
                TopCustomers = GetTopCustomers(period, year, month),
                DailyRevenue = GetDailyRevenue(period, year, month)
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult ExportReport(string period, int year, int month)
        {
            try
            {

                TempData["Success"] = "Đã tải báo cáo thành công!";
                return RedirectToAction("Report");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tạo báo cáo: " + ex.Message;
                return RedirectToAction("Report");
            }
        }
        private void SaveSetting(string key, string value)
        {         
        }

        private List<PaymentTransaction> GetPaymentTransactions()
        {
          
            return new List<PaymentTransaction>
            {
                new PaymentTransaction
                {
                    Id = 1,
                    OrderCode = "ORD001",
                    CustomerName = "Nguyễn Văn A",
                    Amount = 500000,
                    PaymentMethod = "MoMo",
                    Status = "Success",
                    PaymentDate = DateTime.Now.AddDays(-1),
                    TransactionId = "MOMO123456"
                },
                new PaymentTransaction
                {
                    Id = 2,
                    OrderCode = "ORD002",
                    CustomerName = "Trần Thị B",
                    Amount = 750000,
                    PaymentMethod = "VNPay",
                    Status = "Success",
                    PaymentDate = DateTime.Now.AddDays(-2),
                    TransactionId = "VNP789012"
                }
            };
        }

        private decimal CalculateTotalRevenue(string period, int year, int month)
        {
            var transactions = GetPaymentTransactions()
                .Where(p => p.Status == "Success");

            if (period == "month")
            {
                transactions = transactions.Where(p =>
                    p.PaymentDate.Year == year &&
                    p.PaymentDate.Month == month).ToList();
            }
            else if (period == "year")
            {
                transactions = transactions.Where(p =>
                    p.PaymentDate.Year == year).ToList();
            }

            return transactions.Sum(p => p.Amount);
        }

        private int CalculateTotalOrders(string period, int year, int month)
        {
            var transactions = GetPaymentTransactions()
                .Where(p => p.Status == "Success");

            if (period == "month")
            {
                return transactions.Count(p =>
                    p.PaymentDate.Year == year &&
                    p.PaymentDate.Month == month);
            }
            else if (period == "year")
            {
                return transactions.Count(p =>
                    p.PaymentDate.Year == year);
            }

            return transactions.Count();
        }

        private decimal CalculateAverageOrderValue(string period, int year, int month)
        {
            var total = CalculateTotalRevenue(period, year, month);
            var count = CalculateTotalOrders(period, year, month);
            return count > 0 ? total / count : 0;
        }

        private List<PaymentMethodStat> GetPaymentMethodStats(string period, int year, int month)
        {
            var transactions = GetPaymentTransactions()
                .Where(p => p.Status == "Success");

            if (period == "month")
            {
                transactions = transactions.Where(p =>
                    p.PaymentDate.Year == year &&
                    p.PaymentDate.Month == month).ToList();
            }

            return transactions.GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodStat
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount),
                    Percentage = 0 
                }).ToList();
        }

        private List<StatusStat> GetStatusStats(string period, int year, int month)
        {
            var transactions = GetPaymentTransactions();

            return transactions.GroupBy(p => p.Status)
                .Select(g => new StatusStat
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                }).ToList();
        }

        private string GetRevenueChartData(string period, int year, int month)
        {
            return "[]";
        }

        private string GetOrderChartData(string period, int year, int month)
        {
            return "[]";
        }

        private List<TopCustomer> GetTopCustomers(string period, int year, int month)
        {
            var transactions = GetPaymentTransactions()
                .Where(p => p.Status == "Success");

            return transactions.GroupBy(p => p.CustomerName)
                .Select(g => new TopCustomer
                {
                    CustomerName = g.Key,
                    TotalOrders = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(10)
                .ToList();
        }

        private List<DailyRevenueStat> GetDailyRevenue(string period, int year, int month)
        {
            var transactions = GetPaymentTransactions()
                .Where(p => p.Status == "Success" &&
                           p.PaymentDate.Year == year &&
                           p.PaymentDate.Month == month);

            return transactions.GroupBy(p => p.PaymentDate.Date)
                .Select(g => new DailyRevenueStat
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
    public class PaymentGatewayViewModel
    {
        // MoMo
        public bool EnableMomo { get; set; }
        public string MomoPartnerCode { get; set; }
        public string MomoAccessKey { get; set; }
        public string MomoSecretKey { get; set; }
        public string MomoReturnUrl { get; set; }
        public string MomoNotifyUrl { get; set; }

        // VNPay
        public bool EnableVNPay { get; set; }
        public string VNPayTmnCode { get; set; }
        public string VNPayHashSecret { get; set; }
        public string VNPayUrl { get; set; }
        public string VNPayReturnUrl { get; set; }

        // Bank Transfer
        public bool EnableBankTransfer { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string BankSwiftCode { get; set; }

        // COD
        public bool EnableCOD { get; set; }
        public decimal CODFee { get; set; }
        public decimal CODMaxAmount { get; set; }
        public string CODNote { get; set; }

        // ZaloPay
        public bool EnableZaloPay { get; set; }
        public string ZaloPayAppId { get; set; }
        public string ZaloPayKey1 { get; set; }
        public string ZaloPayKey2 { get; set; }

        // PayPal
        public bool EnablePayPal { get; set; }
        public string PayPalClientId { get; set; }
        public string PayPalClientSecret { get; set; }
        public string PayPalMode { get; set; }
    }
    public class PaymentHistoryViewModel
    {
        public List<PaymentTransaction> Transactions { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string SearchKeyword { get; set; }
        public string SelectedStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class PaymentTransaction
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
        public string Note { get; set; }
    }
    public class FinancialReportViewModel
    {
        public string Period { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<PaymentMethodStat> PaymentMethodStats { get; set; }
        public List<StatusStat> StatusStats { get; set; }
        public string RevenueChartData { get; set; }
        public string OrderChartData { get; set; }
        public List<TopCustomer> TopCustomers { get; set; }
        public List<DailyRevenueStat> DailyRevenue { get; set; }
    }

    public class PaymentMethodStat
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    public class StatusStat
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class TopCustomer
    {
        public string CustomerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DailyRevenueStat
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}