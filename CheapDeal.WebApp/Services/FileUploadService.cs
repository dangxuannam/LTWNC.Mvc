using System;
using System.IO;
using System.Linq;
using System.Web;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;

namespace CheapDeal.WebApp.Services
{
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string StoredPath { get; set; }
        public string StoredFileName { get; set; }
        public long FileSizeBytes { get; set; }
    }

    public class FileUploadService
    {
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
        private static readonly string[] ALLOWED_TYPES = { "application/pdf" };
        private static readonly string[] ALLOWED_EXTENSIONS = { ".pdf" };

        private readonly ShopDbContext _db;
        private readonly string _baseUploadPath;

        public FileUploadService(ShopDbContext db)
        {
            _db = db;
            _baseUploadPath = HttpContext.Current.Server.MapPath("~/Uploads/Contracts/");
        }
        public FileUploadResult Upload(HttpPostedFileBase file, int contractId,
                                       string contractCode, string uploadedBy)
        {
            var result = new FileUploadResult();

            // --- Bước 1: Validate file ---
            if (file == null || file.ContentLength == 0)
            {
                result.ErrorMessage = "Vui lòng chọn file PDF";
                return result;
            }

            if (file.ContentLength > MAX_FILE_SIZE)
            {
                result.ErrorMessage = $"File vượt quá {MAX_FILE_SIZE / 1024 / 1024}MB";
                return result;
            }

            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (!ALLOWED_EXTENSIONS.Contains(extension))
            {
                result.ErrorMessage = "Chỉ chấp nhận file PDF";
                return result;
            }

            if (!ALLOWED_TYPES.Contains(file.ContentType))
            {
                result.ErrorMessage = "Loại file không hợp lệ";
                return result;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var safeCode = contractCode.Replace("/", "-").Replace("\\", "-");
            var storedFileName = $"{safeCode}_{timestamp}{extension}";
            var subFolder = Path.Combine(DateTime.Now.Year.ToString(),
                                          DateTime.Now.Month.ToString("00"));
            var fullFolder = Path.Combine(_baseUploadPath, subFolder);
            var fullPath = Path.Combine(fullFolder, storedFileName);

            var storedPath = $"~/Uploads/Contracts/{subFolder}/{storedFileName}";

            try
            {
                if (!Directory.Exists(fullFolder))
                    Directory.CreateDirectory(fullFolder);

                file.SaveAs(fullPath);
                var metadata = new ContractFileMetadata
                {
                    ContractId = contractId,
                    OriginalFileName = file.FileName,
                    StoredPath = storedPath,
                    StoredFileName = storedFileName,
                    FileSizeBytes = file.ContentLength,
                    FileExtension = extension,
                    ContentType = file.ContentType,
                    UploadedBy = uploadedBy,
                    UploadedDate = DateTime.Now,
                    IsActive = true
                };

                _db.ContractFileMetadatas.Add(metadata);

                var contract = _db.Contracts.Find(contractId);
                if (contract != null)
                {
                    contract.DocumentPath = storedPath;
                    contract.UpdatedDate = DateTime.Now;
                }

                _db.SaveChanges();

                result.Success = true;
                result.StoredPath = storedPath;
                result.StoredFileName = storedFileName;
                result.FileSizeBytes = file.ContentLength;
            }
            catch (Exception ex)
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                result.ErrorMessage = $"Lỗi khi lưu file: {ex.Message}";
            }

            return result;
        }

        public bool Delete(int fileId, string deletedBy)
        {
            var metadata = _db.ContractFileMetadatas.Find(fileId);
            if (metadata == null) return false;

            metadata.IsActive = false;
            _db.SaveChanges();
            return true;
        }
    }
}