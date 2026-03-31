using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    
        [Table("Reminders")]
        public class Reminder
        {
            [Key]
            public int ReminderId { get; set; }

            // FK đến ContractPaymentSchedule
            public int ScheduleId { get; set; }

            // FK đến Contract
            public int ContractId { get; set; }

            public DateTime ReminderDate { get; set; }

            [StringLength(50)]
            public string Status { get; set; } // PENDING / SENT

            [StringLength(500)]
            public string Subject { get; set; }

            public string Message { get; set; }

            public DateTime? SentDate { get; set; }

            public DateTime CreatedDate { get; set; }

            // Navigation properties — khớp với ShopDbContext
            public virtual ContractPaymentSchedule Schedule { get; set; }
            public virtual Contract Contract { get; set; }

        }
    }
