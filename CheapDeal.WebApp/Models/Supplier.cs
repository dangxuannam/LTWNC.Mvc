using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class Supplier
    {
        [Display(Name = "Mã nhà cung cấp")]
        public int SupplierId { get; set; }

        [Display(Name = "Tên công ty"), StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)] 
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Người liên hệ")]
        public string ContactName { get; set; }

        [StringLength(20)]
        [Display(Name = "Chức vụ")]
        public string ContactTitle { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Địa chỉ")]
        [DataType(DataType.MultilineText)] 
        public string Address { get; set; }

        [DataType(DataType.EmailAddress)] 
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20), RegularExpression(@"\d{3,4}-\d{3}-\d{4,5}$")]
        [Display(Name = "Số điện thoại")]
        [DataType(DataType.PhoneNumber)] 
        public string Phone { get; set; }

        [StringLength(20)]
        [Display(Name = "Số Fax")]
        [DataType(DataType.PhoneNumber)] 
        public string Fax { get; set; }

        [StringLength(100), DataType(DataType.Url)] 
        [Display(Name = "Trang chủ (Website)")]
        public string HomePage { get; set; }

        [Display(Name = "Đang hợp tác")]
        public bool Actived { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual IList<Product> Products { get; set; }
    }
}