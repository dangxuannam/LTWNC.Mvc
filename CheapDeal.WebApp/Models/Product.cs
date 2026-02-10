using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Models
{
    public class Product
    {
        [Display(Name = "Mã ID")]
        public int ProductId { get; set; }

        [Required, StringLength(100), Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Required, StringLength(100), Display(Name = "Bí danh (SEO)")]
        public string Alias { get; set; }

        [Required, StringLength(300), Display(Name = "Hình đại diện")]
        [DataType(DataType.ImageUrl)] 
        public string ThumbImage { get; set; }

        [Required, StringLength(20), Display(Name = "Mã SKU/Code")]
        public string ProductCode { get; set; }

        [StringLength(500), Display(Name = "Tóm tắt")]
        [DataType(DataType.MultilineText)] 
        public string ShortIntro { get; set; }

        [Column(TypeName = "ntext"), AllowHtml, Display(Name = "Mô tả chi tiết")]
        [DataType(DataType.Html)] 
        public string Description { get; set; }

        [StringLength(50), Display(Name = "Quy cách")]
        public string QtyPerUnit { get; set; }

        [Display(Name = "Đơn giá")]
        [DataType(DataType.Currency)] 
        public int Price { get; set; }

        [Display(Name = "Số lượng tồn")]
        public int Quantity { get; set; }

        [Range(0, 1), DisplayFormat(DataFormatString = "{0:P1}")]
        
        public float Discount { get; set; }

        [Display(Name = "Mã nhà cung cấp")]
        public int SupplierId { get; set; }

        [Display(Name = "Trạng thái kích hoạt")]
        public bool Actived { get; set; }

        [Display(Name = "Mã hồ sơ sản phẩm")]
        public int ProductProfileId { get; set; }

        [Display(Name = "Thông số thống kê")]

        [Timestamp, JsonIgnore]
        public byte[] RowVersion { get; set; }

        [JsonIgnore]
        public virtual Supplier Supplier { get; set; }
        [JsonIgnore]
        public virtual IList<Category> Categories { get; set; }
        [JsonIgnore]
        public virtual IList<OrderDetail> OrderDetails { get; set; }
        [JsonIgnore]
        public virtual IList<Picture> Pictures { get; set; }
        [JsonIgnore]
        public virtual IList<ProductHistory> ProductHistories { get; set; }
        public int CategoryID { get; internal set; }
    }
}