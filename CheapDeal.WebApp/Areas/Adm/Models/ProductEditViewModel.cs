using CheapDeal.Core.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Models
{
    public class ProductEditViewModel : ProductCreateViewModel
    {
        public int ProductId { get; set; } 
        public string ThumbImage { get; set; } 
        public byte[] RowVersion { get; set; }
    }

}