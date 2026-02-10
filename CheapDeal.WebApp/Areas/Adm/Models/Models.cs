using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheapDeal.WebApp.Models
{

    [Table("Settings")]
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Index(IsUnique = true)]
        public string Key { get; set; }

        public string Value { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; }

        public Setting()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
        }
    }

    [Table("BackupHistory")]
    public class BackupHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [StringLength(500)]
        public string FilePath { get; set; }

        public DateTime BackupDate { get; set; }

        public long? FileSize { get; set; }

        [StringLength(20)]
        public string FileSizeFormatted { get; set; }

        [StringLength(50)]
        public string BackupType { get; set; } 

        [StringLength(20)]
        public string Status { get; set; }  

        [StringLength(100)]
        public string CreatedBy { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? RestoredDate { get; set; }

        [StringLength(100)]
        public string RestoredBy { get; set; }

        public BackupHistory()
        {
            BackupDate = DateTime.Now;
            BackupType = "Manual";
            Status = "Success";
            IsDeleted = false;
        }
    }

    [Table("AuditLog")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [StringLength(50)]
        public string EntityType { get; set; }

        [StringLength(50)]
        public string EntityId { get; set; }

        public string OldValue { get; set; }  // JSON

        public string NewValue { get; set; }  // JSON

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }

        public AuditLog()
        {
            CreatedDate = DateTime.Now;
            IsSuccess = true;
        }
    }
}