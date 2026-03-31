using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class ContractPaymentSchedule
    {
        [Key]
        public int ScheduleId { get; set; }
        public int ContractId { get; set; }
        public int InstallmentNo { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? PaidAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public virtual Contract Contract { get; set; }
    }
}