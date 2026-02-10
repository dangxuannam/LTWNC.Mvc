using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheapDeal.WebApp.Models
{
    public enum PaymentMethod
    {
        [Display(Name = "Ví MoMo")]
        MoMo,
        [Display(Name = "VNPay")]
        VNPay,
        [Display(Name = "Chuyển khoản")]
        BankTransfer,
        [Display(Name = "COD")]
        COD,
        [Display(Name = "ZaloPay")]
        ZaloPay,
        [Display(Name = "PayPal")]
        PayPal
    }

    public enum PaymentStatus
    {
        [Display(Name = "Chờ thanh toán")]
        Pending,
        [Display(Name = "Thành công")]
        Success,
        [Display(Name = "Thất bại")]
        Failed,
        [Display(Name = "Đã hủy")]
        Cancelled,
        [Display(Name = "Hoàn tiền")]
        Refunded
    }

    public class Payment
    {
        [Key]
        [Display(Name = "Mã thanh toán")]
        public int PaymentId { get; set; }

        [Display(Name = "Mã đơn hàng")]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Trạng thái")]
        public PaymentStatus Status { get; set; }

        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        [Display(Name = "Mã giao dịch")]
        public string TransactionId { get; set; }

        [StringLength(500)]
        [Display(Name = "Thông tin thanh toán")]
        public string PaymentInfo { get; set; }

        [Display(Name = "Thời gian tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Thời gian thanh toán")]
        public DateTime? PaidDate { get; set; }

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        [StringLength(2000)]
        [Display(Name = "Dữ liệu phản hồi")]
        public string ResponseData { get; set; }

        public virtual Order Order { get; set; }
    }
}