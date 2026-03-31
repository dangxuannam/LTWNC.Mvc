using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using OfficeOpenXml;   // EPPlus namespace

namespace CheapDeal.WebApp.Services
{
    public class ExcelImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<int> CreatedContractIds { get; set; } = new List<int>();
    }

    public class ExcelImportService
    {
        private readonly ShopDbContext _db;

        public ExcelImportService(ShopDbContext db)
        {
            _db = db;
        }

        public List<ContractImportRow> ReadExcel(Stream fileStream)
        {
            var rows = new List<ContractImportRow>();

            using (var package = new ExcelPackage(fileStream))
            {
                var sheet = package.Workbook.Worksheets[1]; 
                int lastRow = sheet.Dimension.End.Row;

                for (int r = 2; r <= lastRow; r++)
                {
                    // Bỏ qua dòng trống
                    if (sheet.Cells[r, 1].Value == null) continue;

                    var row = new ContractImportRow { RowNumber = r };

                    try
                    {
                        row.ContractCode = sheet.Cells[r, 1].GetValue<string>()?.Trim();
                        row.CustomerEmail = sheet.Cells[r, 2].GetValue<string>()?.Trim();
                        row.ContractDate = ParseDate(sheet.Cells[r, 3].GetValue<string>(), r, "ContractDate");
                        row.StartDate = ParseDate(sheet.Cells[r, 4].GetValue<string>(), r, "StartDate");
                        row.EndDate = ParseDate(sheet.Cells[r, 5].GetValue<string>(), r, "EndDate");
                        row.TotalAmount = sheet.Cells[r, 6].GetValue<decimal>();
                        row.InstallmentCount = sheet.Cells[r, 7].GetValue<int?>() ?? 1;
                        row.Terms = sheet.Cells[r, 8].GetValue<string>();
                    }
                    catch (Exception ex)
                    {
                        row.IsValid = false;
                        row.ErrorMessage = $"Dòng {r}: Lỗi đọc dữ liệu - {ex.Message}";
                    }

                    rows.Add(row);
                }
            }

            return rows;
        }

        public void Validate(List<ContractImportRow> rows)
        {
            // Lấy danh sách code đã tồn tại trong DB
            var existingCodes = new HashSet<string>(
             _db.Contracts.Select(c => c.ContractCode).ToList()
             );

            // Theo dõi code trong file để phát hiện trùng nội bộ
            var codesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                if (!row.IsValid) continue;

                var errors = new List<string>();

                // Validate ContractCode
                if (string.IsNullOrEmpty(row.ContractCode))
                    errors.Add("Mã hợp đồng trống");
                else if (existingCodes.Contains(row.ContractCode))
                    errors.Add($"Mã '{row.ContractCode}' đã tồn tại trong DB");
                else if (!codesInFile.Add(row.ContractCode))
                    errors.Add($"Mã '{row.ContractCode}' bị trùng trong file Excel");

                // Validate CustomerEmail tồn tại
                if (string.IsNullOrEmpty(row.CustomerEmail))
                    errors.Add("Email khách hàng trống");
                else
                {
                    bool emailExists = _db.Users
                        .Any(u => u.Email == row.CustomerEmail);
                    if (!emailExists)
                        errors.Add($"Email '{row.CustomerEmail}' không tìm thấy trong hệ thống");
                }

                // Validate ngày
                if (row.EndDate <= row.StartDate)
                    errors.Add("EndDate phải sau StartDate");

                // Validate số tiền
                if (row.TotalAmount <= 0)
                    errors.Add("TotalAmount phải > 0");

                // Validate kỳ
                if (row.InstallmentCount < 1 || row.InstallmentCount > 36)
                    errors.Add("Số kỳ phải từ 1 đến 36");

                if (errors.Any())
                {
                    row.IsValid = false;
                    row.ErrorMessage = string.Join("; ", errors);
                }
            }
        }
        public ExcelImportResult Import(List<ContractImportRow> rows, string createdBy)
        {
            var result = new ExcelImportResult
            {
                TotalRows = rows.Count,
                ErrorCount = rows.Count(r => !r.IsValid)
            };

            // Ghi lỗi validate vào result
            foreach (var row in rows.Where(r => !r.IsValid))
                result.Errors.Add($"Dòng {row.RowNumber}: {row.ErrorMessage}");

            // Xử lý từng dòng hợp lệ
            foreach (var row in rows.Where(r => r.IsValid))
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        // Lấy CustomerId từ Email
                        var customerId = _db.Users
                            .Where(u => u.Email == row.CustomerEmail)
                            .Select(u => u.Id)
                            .FirstOrDefault();

                        // Gọi SP tạo hợp đồng (đã tạo tuần 9)
                        var newIdSql = @"DECLARE @newId INT
                            EXEC dbo.sp_CreateContract
                                @CustomerId=@p0, @ContractCode=@p1, @ContractDate=@p2,
                                @StartDate=@p3, @EndDate=@p4, @TotalAmount=@p5,
                                @InstallmentCount=@p6, @CreatedBy=@p7,
                                @NewContractId=@newId OUTPUT
                            SELECT @newId";

                        int newId = _db.Database.SqlQuery<int>(newIdSql,
                            customerId, row.ContractCode, row.ContractDate,
                            row.StartDate, row.EndDate, row.TotalAmount,
                            row.InstallmentCount, createdBy).FirstOrDefault();

                        transaction.Commit();
                        result.SuccessCount++;
                        result.CreatedContractIds.Add(newId);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.ErrorCount++;
                        result.Errors.Add($"Dòng {row.RowNumber} ({row.ContractCode}): {ex.Message}");
                    }
                }
            }

            return result;
        }

        // Helper: parse ngày từ string
        private DateTime ParseDate(string value, int row, string field)
        {
            if (DateTime.TryParseExact(value,
                new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
                null, System.Globalization.DateTimeStyles.None, out DateTime result))
                return result;

            throw new FormatException($"Cột {field} dòng {row}: '{value}' không đúng định dạng ngày (dd/MM/yyyy)");
        }
    }
}