using CheapDeal.Core.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Models
{
    [Bind(Exclude = "Categories,Suppliers")]
    public class ProductCreateViewModel
    {
        [Required, StringLength(100), Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Required, StringLength(100), Display(Name = "Tên định danh")]
        [Remote("CheckUniqueAlias", "Product", AdditionalFields = "ProductId",
            ErrorMessage = "{0} này đã được sử dụng cho sản phẩm khác")]
        public string Alias { get; set; }   
        [Required, StringLength(20), Display(Name = "Số hiệu sản phẩm")]
        [Remote("CheckUniqueCode", "Product", AdditionalFields = "ProductId",
            ErrorMessage = "{0} này đã được sử dụng cho sản phẩm khác")]
        public string ProductCode { get; set; }

        [Display(Name = "Hình đại diện")]
        [FileType("jpg,jpeg,png,gif"), FileSize(1)]
        [ImageSize(Width = 400, Height = 500)]
        public HttpPostedFileBase Upload { get; set; } 

        [StringLength(500), DataType(DataType.MultilineText)]
        [Display(Name = "Giới thiệu ")]
        public string ShortIntro { get; set; }

        [AllowHtml, Display(Name = "Mô tả chi tiết")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(50), Display(Name = "Đơn vị tính")]
        public string QtyPerUnit { get; set; }

        [Display(Name = "Giá bán"), Range(1000, 100000000)]
        public int Price { get; set; }

        [Display(Name = "Số lượng"), Range(0, 1000000)]
        public int Quantity { get; set; }

        [Range(0, 100000000), Display(Name = " Giảm giá ")]
        public float Discount { get; set; } 

        [Display(Name = "Nhà cung cấp")] 
        public int SupplierId { get; set; }

        [Display(Name = "Trạng thái")] 
        public int Actived { get; set; }
        public int[] SelectedCategories { get; set; }
        public MultiSelectList Categories { get; set; }
        public SelectList Products { get; set; }

        public SelectList Suppliers { get; set; }
    }
}