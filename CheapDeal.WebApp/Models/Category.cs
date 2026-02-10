using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class Category
    {
        [Display(Name = "Mã danh mục")]
        public int CategoryId { get; set; }

        [Required, Display(Name = "Tên danh mục"), StringLength(100)]
        public string Name { get; set; }

        [Required, Display(Name = "(Alias)"), StringLength(100)]
        public string Alias { get; set; }

        [Required, Display(Name = "Mô tả"), StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(500), Display(Name = "biểu tượng")]
        [DataType(DataType.ImageUrl)]
        public string IconPath { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool Actived { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentID { get; set; }

        [Display(Name = "Thứ tự sắp xếp")]
        public int OrderNo { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Category Parent { get; set; }
        public virtual IList<Product> Products { get; set; }
        public virtual IList<Category> ChildCates { get; set; }
    }
}