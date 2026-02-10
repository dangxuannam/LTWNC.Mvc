using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Mới")]
        New,
        [Display(Name = "Đã hủy")]
        Cancelled,
        [Display(Name = "Đang giao hàng")]
        Shipping,
        [Display(Name = "Đã trả hàng")]
        Returned,
        [Display(Name = "Đã xác nhận")]
        Approved,
        [Display(Name = "Thành công")]
        Success
    }

    public class Order
    {
        public Order()
        {
            OrderDetails = new List<OrderDetail>();
            Status = OrderStatus.New;
            OrderDate = DateTime.Now;
            Freight = 0;
        }

        [Display(Name = "Mã đơn hàng")]
        public int OrderId { get; set; }

        [StringLength(128)]
        [Display(Name = "Mã khách hàng")]
        public string CustomerId { get; set; }

        [StringLength(128)]
        [Display(Name = "Mã nhân viên xử lý")]
        public string EmployeeId { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        [DataType(DataType.Date)]
        public DateTime? OrderDate { get; set; }

        [Display(Name = "Ngày yêu cầu giao")]
        [DataType(DataType.Date)]
        public DateTime? RequiredDate { get; set; }

        [Display(Name = "Ngày thực tế giao")]
        [DataType(DataType.Date)]
        public DateTime? ShipDate { get; set; }

        [Display(Name = "Phí vận chuyển")]
        [DataType(DataType.Currency)]
        public int Freight { get; set; }

        [Display(Name = "Mã đơn vị vận chuyển")]
        public int? ShipVia { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Tên người nhận")]
        public string ShipName { get; set; }

        [Required, StringLength(300)]
        [Display(Name = "Địa chỉ nhận hàng")]
        [DataType(DataType.MultilineText)]
        public string ShipAddress { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Số điện thoại nhận")]
        [DataType(DataType.PhoneNumber)]
        public string ShipTel { get; set; }

        [Display(Name = "Trạng thái đơn hàng")]
        public OrderStatus Status { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú đơn hàng")]
        public string Notes { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TotalPrice { get; set; }

        public byte[] RowVersion { get; set; }

        public virtual Account Customer { get; set; }
        public virtual Account Employee { get; set; }
        public virtual Shipper Shipper { get; set; }

        public virtual IList<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}