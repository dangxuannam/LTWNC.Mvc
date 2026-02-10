using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    [Table("ShippingRates")]
    public class ShippingRate
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Shipper")]
        public int ShipperId { get; set; }

        [Display(Name = "Khu vực")]
        public string ProvinceName { get; set; } 

        [Display(Name = "Từ (kg)")]
        public double MinWeight { get; set; }

        [Display(Name = "Đến (kg)")]
        public double MaxWeight { get; set; }

        [Required]
        [Display(Name = "Phí ship")]
        public decimal Price { get; set; }

        public virtual Shipper Shipper { get; set; }
    }
}
    