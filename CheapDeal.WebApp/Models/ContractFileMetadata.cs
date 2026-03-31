using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class ContractFileMetadata
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required, StringLength(255)]
        public string OriginalFileName { get; set; }

        [Required, StringLength(500)]
        public string StoredPath { get; set; }

        [Required, StringLength(500)]
        public string StoredFileName { get; set; }

        public long FileSizeBytes { get; set; }

        [StringLength(10)]
        public string FileExtension { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        public string UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Contract Contract { get; set; }
    }
}