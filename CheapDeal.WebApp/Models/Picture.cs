using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class Picture
    {
        [Display(Name = "Mã ảnh")]
        public int PictureId { get; set; }

        [StringLength(150), Required]
        [Display(Name = "Chú thích ảnh")]
        public string Caption { get; set; }

        [StringLength(500), Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Đường dẫn ảnh")]
        public string Path { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int OrderNo { get; set; }

        [Display(Name = "Đang sử dụng")]
        public bool Actived { get; set; }

        [Display(Name = "Mã sản phẩm")]
        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}