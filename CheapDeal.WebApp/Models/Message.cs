using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        [StringLength(128)]
        public string SenderId { get; set; }

        [Required]
        [StringLength(128)]
        public string ReceiverId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Đã đọc")]
        public bool IsRead { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime SentDate { get; set; }

        [Display(Name = "Ngày đọc")]
        public DateTime? ReadDate { get; set; }

        [Display(Name = "Người gửi đã xóa")]
        public bool IsDeletedBySender { get; set; }

        [Display(Name = "Người nhận đã xóa")]
        public bool IsDeletedByReceiver { get; set; }

        [ForeignKey("SenderId")]
        public virtual Account Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Account Receiver { get; set; }
    }
}