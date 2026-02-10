using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class AllModels
    {

        [Table("Notifications")]
        public class Notification
        {
            [Key]
            public int NotificationId { get; set; }

            [Required]
            [StringLength(450)]
            public string AccountId { get; set; }

            [Required]
            [StringLength(200)]
            [Display(Name = "Tiêu đề")]
            public string Title { get; set; }

            [Required]
            [StringLength(1000)]
            [Display(Name = "Nội dung")]
            public string Content { get; set; }

            [StringLength(50)]
            [Display(Name = "Loại thông báo")]
            public string Type { get; set; }

            [StringLength(200)]
            [Display(Name = "Liên kết")]
            public string Link { get; set; }

            [Display(Name = "Đã đọc")]
            public bool IsRead { get; set; }

            [Display(Name = "Ngày tạo")]
            public DateTime CreatedDate { get; set; }

            [Display(Name = "Ngày đọc")]
            public DateTime? ReadDate { get; set; }

            [ForeignKey("AccountId")]
            public virtual Account Account { get; set; }

            public Notification()
            {
                CreatedDate = DateTime.Now;
                IsRead = false;
                Type = "System";
            }
        }

        [Table("UserActivities")]
        public class UserActivity
        {
            [Key]
            public int ActivityId { get; set; }

            [Required]
            [StringLength(450)]
            public string AccountId { get; set; }

            [Required]
            [StringLength(100)]
            [Display(Name = "Loại hoạt động")]
            public string ActivityType { get; set; }

            [StringLength(500)]
            [Display(Name = "Chi tiết")]
            public string Details { get; set; }

            [Display(Name = "Thời gian")]
            public DateTime Timestamp { get; set; }

            [StringLength(50)]
            [Display(Name = "Địa chỉ IP")]
            public string IpAddress { get; set; }

            [StringLength(500)]
            [Display(Name = "User Agent")]
            public string UserAgent { get; set; }

            [ForeignKey("AccountId")]
            public virtual Account Account { get; set; }

            public UserActivity()
            {
                Timestamp = DateTime.Now;
            }
        }
    }
}