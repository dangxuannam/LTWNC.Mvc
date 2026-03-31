using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using CheapDeal.WebApp.Services;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ContractController : Controller
    {
        private readonly ShopDbContext _db = new ShopDbContext();
        private readonly ExcelImportService _excelService;
        private readonly FileUploadService _fileService;

        public ContractController()
        {
            _excelService = new ExcelImportService(_db);
            _fileService = new FileUploadService(_db);
        }
        public ActionResult Index(int page = 1, string status = "", string search = "")
        {
            const int pageSize = 10;

            var query = _db.Contracts
                .Include("Status")
                .Include("Customer")
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status.StatusCode == status);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c =>
                    c.ContractCode.Contains(search) ||
                    c.Customer.Email.Contains(search));

            var total = query.Count();
            var contracts = query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.TotalContracts = total;
            ViewBag.StatusList = _db.ContractStatuses
                                        .OrderBy(s => s.DisplayOrder).ToList();
            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;

            return View(contracts);
        }

        public ActionResult Detail(int id)
        {
            var contract = _db.Contracts
                .Include("Status")
                .Include("Customer")
                .Include("Schedules")
                .FirstOrDefault(c => c.ContractId == id);

            if (contract == null) return HttpNotFound();

            ViewBag.Files = _db.ContractFileMetadatas
                .Where(f => f.ContractId == id && f.IsActive)
                .OrderByDescending(f => f.UploadedDate)
                .ToList();

            ViewBag.StatusList = _db.ContractStatuses
                .OrderBy(s => s.DisplayOrder).ToList();

            return View(contract);
        }

        public ActionResult ImportExcel()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcel(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel");
                return View();
            }

            var ext = Path.GetExtension(excelFile.FileName)?.ToLower();
            if (ext != ".xlsx" && ext != ".xls")
            {
                ModelState.AddModelError("", "Chỉ nhận file .xlsx hoặc .xls");
                return View();
            }

            if (excelFile.ContentLength > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File vượt quá 5MB");
                return View();
            }

            try
            {
                var rows = _excelService.ReadExcel(excelFile.InputStream);
                _excelService.Validate(rows);
                TempData["ImportRows"] = rows;
                return View("ImportPreview", rows);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi đọc file: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExcelAjax(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0)
                return Json(new { success = false, message = "Vui lòng chọn file Excel" });

            var ext = Path.GetExtension(excelFile.FileName)?.ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                return Json(new { success = false, message = "Chỉ nhận file .xlsx hoặc .xls" });

            if (excelFile.ContentLength > 5 * 1024 * 1024)
                return Json(new { success = false, message = "File vượt quá 5MB" });

            try
            {
                var rows = _excelService.ReadExcel(excelFile.InputStream);
                _excelService.Validate(rows);
                TempData["ImportRows"] = rows;

                var data = rows.Select(r => new {
                    rowNumber = r.RowNumber,
                    contractCode = r.ContractCode,
                    customerEmail = r.CustomerEmail,
                    contractDate = r.ContractDate.ToString("dd/MM/yyyy"),
                    startDate = r.StartDate.ToString("dd/MM/yyyy"),
                    endDate = r.EndDate.ToString("dd/MM/yyyy"),
                    totalAmount = r.TotalAmount,
                    installmentCount = r.InstallmentCount,
                    isValid = r.IsValid,
                    errorMessage = r.ErrorMessage
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = data,
                    validCount = rows.Count(r => r.IsValid),
                    errorCount = rows.Count(r => !r.IsValid)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi đọc file: " + ex.Message });
            }
        }

        public ActionResult ImportPreview()
        {
            var rows = TempData["ImportRows"] as List<ContractImportRow>;
            if (rows == null) return RedirectToAction("ImportExcel");
            TempData.Keep("ImportRows");
            return View(rows);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmImport()
        {
            var rows = TempData["ImportRows"] as List<ContractImportRow>;
            if (rows == null) return RedirectToAction("ImportExcel");

            var result = _excelService.Import(rows, User.Identity.GetUserId());
            TempData["ImportResult"] = result;
            return RedirectToAction("ImportResult");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmImportAjax()
        {
            var rows = TempData["ImportRows"] as List<ContractImportRow>;

            if (rows == null)
                return Json(new
                {
                    success = false,
                    message = "Phiên làm việc hết hạn, vui lòng upload lại file."
                });

            try
            {
                var result = _excelService.Import(rows, User.Identity.GetUserId());

                return Json(new
                {
                    success = true,
                    result = new
                    {
                        totalRows = result.TotalRows,
                        successCount = result.SuccessCount,
                        errorCount = result.ErrorCount,
                        errors = result.Errors,
                        createdIds = result.CreatedContractIds
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi import: " + ex.Message });
            }
        }
        public ActionResult ImportResult()
        {
            var result = TempData["ImportResult"] as ExcelImportResult;
            if (result == null) return RedirectToAction("Index");
            return View(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadPdf(int contractId, HttpPostedFileBase pdfFile)
        {
            var contract = _db.Contracts.Find(contractId);
            if (contract == null) return HttpNotFound();

            var result = _fileService.Upload(
                pdfFile,
                contractId,
                contract.ContractCode,
                User.Identity.GetUserId()
            );

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    success = result.Success,
                    message = result.Success ? "Upload PDF thành công!" : result.ErrorMessage,
                    storedPath = result.StoredPath,
                    fileName = result.StoredFileName,
                    fileSizeKb = result.FileSizeBytes / 1024
                });
            }

            if (result.Success)
                TempData["SuccessMessage"] = "Upload file PDF thành công!";
            else
                TempData["ErrorMessage"] = result.ErrorMessage;

            return RedirectToAction("Detail", new { id = contractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFile(int fileId, int contractId)
        {
            var file = _db.ContractFileMetadatas
                .FirstOrDefault(f => f.FileId == fileId && f.ContractId == contractId);

            if (file == null)
                return Json(new { success = false, message = "File không tồn tại" });

            file.IsActive = false;
            _db.SaveChanges();

            return Json(new { success = true, message = "Đã xóa file thành công" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int contractId, int newStatusId, string notes = "")
        {
            try
            {
                _db.Database.ExecuteSqlCommand(
                    "EXEC dbo.sp_UpdateContractStatus @p0, @p1, @p2, @p3, @p4",
                    contractId,
                    newStatusId,
                    User.Identity.GetUserId(),
                    notes,
                    Request.UserHostAddress
                );

                var newStatus = _db.ContractStatuses.Find(newStatusId);

                return Json(new
                {
                    success = true,
                    message = "Cập nhật trạng thái thành công",
                    newStatusName = newStatus?.StatusName,
                    newStatusColor = newStatus?.Color
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult DownloadTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("HopDong");

                string[] headers = {
                    "ContractCode", "CustomerEmail", "ContractDate(dd/MM/yyyy)",
                    "StartDate(dd/MM/yyyy)", "EndDate(dd/MM/yyyy)",
                    "TotalAmount", "InstallmentCount(1-36)", "Terms"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = sheet.Cells[1, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor
                        .SetColor(System.Drawing.Color.FromArgb(30, 58, 138));
                    cell.Style.Font.Color
                        .SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                }

                sheet.Cells[2, 1].Value = "HD-2026-001";
                sheet.Cells[2, 2].Value = "khachhang@example.com";
                sheet.Cells[2, 3].Value = "01/03/2026";
                sheet.Cells[2, 4].Value = "01/04/2026";
                sheet.Cells[2, 5].Value = "31/12/2026";
                sheet.Cells[2, 6].Value = 12000000;
                sheet.Cells[2, 7].Value = 3;
                sheet.Cells[2, 8].Value = "Thanh toán theo quý";

                sheet.Cells[2, 1, 2, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[2, 1, 2, 8].Style.Fill.BackgroundColor
                    .SetColor(System.Drawing.Color.FromArgb(239, 246, 255));

                sheet.Cells.AutoFitColumns();

                return File(
                    package.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Template_Import_HopDong.xlsx"
                );
            }
        }

        public ActionResult ExportExcel(string search = "", string status = "")
        {
            var query = _db.Contracts
                .Include("Status")
                .Include("Customer")
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status.StatusCode == status);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c =>
                    c.ContractCode.Contains(search) ||
                    c.Customer.Email.Contains(search));

            var contracts = query.OrderByDescending(c => c.CreatedDate).ToList();

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("DanhSachHopDong");

                string[] headers = {
                    "STT", "Mã HĐ", "Khách hàng", "Email",
                    "Ngày ký", "Bắt đầu", "Kết thúc",
                    "Tổng tiền (đ)", "Đã TT (đ)", "Còn lại (đ)",
                    "Trạng thái", "Ngày tạo"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = sheet.Cells[1, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor
                        .SetColor(System.Drawing.Color.FromArgb(30, 58, 138));
                    cell.Style.Font.Color
                        .SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < contracts.Count; i++)
                {
                    var c = contracts[i];
                    int row = i + 2;

                    sheet.Cells[row, 1].Value = i + 1;
                    sheet.Cells[row, 2].Value = c.ContractCode;
                    sheet.Cells[row, 3].Value = c.Customer?.UserName ?? "";
                    sheet.Cells[row, 4].Value = c.Customer?.Email ?? "";
                    sheet.Cells[row, 5].Value = c.ContractDate.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 6].Value = c.StartDate.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 7].Value = c.EndDate.ToString("dd/MM/yyyy");
                    sheet.Cells[row, 8].Value = (double)c.TotalAmount;
                    sheet.Cells[row, 9].Value = (double)c.PaidAmount;
                    sheet.Cells[row, 10].Value = (double)(c.TotalAmount - c.PaidAmount);
                    sheet.Cells[row, 11].Value = c.Status?.StatusName ?? "";
                    sheet.Cells[row, 12].Value = c.CreatedDate.ToString("dd/MM/yyyy HH:mm");
                    sheet.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[row, 10].Style.Numberformat.Format = "#,##0";

                    if (i % 2 == 1)
                    {
                        sheet.Cells[row, 1, row, headers.Length].Style.Fill.PatternType =
                            ExcelFillStyle.Solid;
                        sheet.Cells[row, 1, row, headers.Length].Style.Fill.BackgroundColor
                            .SetColor(System.Drawing.Color.FromArgb(239, 246, 255));
                    }

                    for (int col = 8; col <= 10; col++)
                        sheet.Cells[row, col].Style.HorizontalAlignment =
                            ExcelHorizontalAlignment.Right;
                }
                int sumRow = contracts.Count + 2;
                sheet.Cells[sumRow, 7].Value = "TỔNG CỘNG:";
                sheet.Cells[sumRow, 7].Style.Font.Bold = true;
                sheet.Cells[sumRow, 8].Formula = $"SUM(H2:H{sumRow - 1})";
                sheet.Cells[sumRow, 9].Formula = $"SUM(I2:I{sumRow - 1})";
                sheet.Cells[sumRow, 10].Formula = $"SUM(J2:J{sumRow - 1})";
                for (int col = 8; col <= 10; col++)
                {
                    sheet.Cells[sumRow, col].Style.Font.Bold = true;
                    sheet.Cells[sumRow, col].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[sumRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[sumRow, col].Style.Fill.BackgroundColor
                        .SetColor(System.Drawing.Color.FromArgb(254, 243, 199));
                }

                sheet.Cells.AutoFitColumns();

                var fileName = $"DanhSachHopDong_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(
                    package.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
