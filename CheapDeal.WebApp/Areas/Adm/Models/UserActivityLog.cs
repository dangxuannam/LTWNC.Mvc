using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.EnterpriseServices;

namespace CheapDeal.WebApp.Models
{
    [Table("UserActivityLogs")]
    public class UserActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string AccountId { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }
    }
}
