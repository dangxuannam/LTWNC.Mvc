using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }
        public string ContractCode { get; set; }
        public string CustomerId { get; set; }
        public int? OrderId { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int StatusId { get; set; }
        public string Terms { get; set; }
        public string DocumentPath { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public byte[] RowVersion { get; set; }

        // Navigation - dùng Account thay ApplicationUser
        public virtual ContractStatus Status { get; set; }
        public virtual Account Customer { get; set; }
        public virtual ICollection<ContractPaymentSchedule> Schedules { get; set; }
    }

}
