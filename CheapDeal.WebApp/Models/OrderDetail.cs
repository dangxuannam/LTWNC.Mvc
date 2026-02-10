using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class OrderDetail
    {
        [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Mã đơn hàng")]
        public int OrderId { get; set; }

        [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Mã sản phẩm")]
        public int ProductId { get; set; }

        [Display(Name = "Đơn giá")]
        [DataType(DataType.Currency)]
        public int Price { get; set; }

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Chiết khấu")]
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public float Discount { get; set; }

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Thành tiền")]
        [DataType(DataType.Currency)]
        public int Total
        {
            get { return (int)Math.Round(Quantity * Price * (1 - Discount), 0); }
        }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}