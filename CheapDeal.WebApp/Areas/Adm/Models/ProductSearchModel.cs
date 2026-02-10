using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Areas.Adm.Models
{
    public class ProductSearchModel
    {
        public string Keyword {  get; set; }

        public int? PageIndex  { get; set; }
        public int? PageSize { get; set; }

        public bool? Actived { get; set; }
    }
}