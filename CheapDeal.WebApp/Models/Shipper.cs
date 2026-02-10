using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class Shipper
    {
        [Display(Name = "Mã đơn vị vận chuyển")]
        public int ShipperID { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Tên công ty vận chuyển")]
        public string CompanyName { get; set; }

        [Required, StringLength(20)]
        [RegularExpression(@"\d{3,4}-\d{3}-\d{4,5}$")]
        [Display(Name = "Số điện thoại")]
        [DataType(DataType.PhoneNumber)] 
        public string Phone { get; set; }

        [Required, StringLength(300)]
        [Display(Name = "Địa chỉ văn phòng")]
        [DataType(DataType.MultilineText)] 
        public string Address { get; set; }

        public virtual IList<Order> Orders { get; set; }
    }
}