using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class ContractImportRow
    {
        public int RowNumber { get; set; }           // Số thứ tự dòng trong Excel (để báo lỗi)

        [Required(ErrorMessage = "Mã hợp đồng không được trống")]
        [StringLength(50)]
        public string ContractCode { get; set; }

        [Required(ErrorMessage = "Email khách hàng không được trống")]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        public DateTime ContractDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Tổng tiền phải > 0")]
        public decimal TotalAmount { get; set; }

        [Range(1, 36)]
        public int InstallmentCount { get; set; } = 1;

        public string Terms { get; set; }

        // Kết quả sau khi xử lý
        public bool IsValid { get; set; } = true;
        public string ErrorMessage { get; set; }
    }
}