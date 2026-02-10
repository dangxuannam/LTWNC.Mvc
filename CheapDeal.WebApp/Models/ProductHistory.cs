using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public enum ProductHistoryAction
    {
        [Display(Name = "Đã tạo mới")]
        Create,
        [Display(Name = "Đã cập nhật")]
        Updated,
        [Display(Name = "Cập nhật toàn bộ")]
        UpdateFull,
        [Display(Name = "Cập nhật giá")]
        UpdatePrice,
        [Display(Name = "Cập nhật số lượng")]
        UpdateQuantity
    }

    public class ProductHistory
    {
        [Display(Name = "Mã lịch sử")]
        public int Id { get; set; }

        [Display(Name = "Mã sản phẩm")]
        public int ProductId { get; set; }

        [StringLength(128)]
        [Display(Name = "Mã tài khoản thực hiện")]
        public string AccountId { get; set; }

        [Display(Name = "Thời điểm thực hiện")]
        [DataType(DataType.DateTime)] 
        public DateTime ActionTime { get; set; }

        [Display(Name = "Loại hành động")]
        public ProductHistoryAction HistoryAction { get; set; }

        [Column(TypeName = "ntext")]
        [Display(Name = "Dữ liệu sản phẩm gốc")]
        [DataType(DataType.MultilineText)]
        public string OriginalProduct { get; set; }

        [Column(TypeName = "ntext")]
        [Display(Name = "Mô tả chi tiết")]
        [DataType(DataType.MultilineText)] 
        public string Description { get; set; }

        [Column(TypeName = "ntext")]
        [Display(Name = "Dữ liệu sản phẩm thay đổi")]
        [DataType(DataType.MultilineText)] 
        public string ModifiedProduct { get; set; }

        public virtual Product Product { get; set; }
        public virtual Account Account { get; set; }
    }
}