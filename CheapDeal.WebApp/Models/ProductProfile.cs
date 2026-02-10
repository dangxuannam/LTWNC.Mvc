using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    public class ProductProfile
    {
        [Display(Name = "Mã hồ sơ")]
        public int ProductProfileId { get; set; }

        [Display(Name = "Số lần bình chọn")]
        [DisplayFormat(DataFormatString = "{0:N0}")] 
        public int VoteCount { get; set; }

        [Display(Name = "Tổng điểm")]
        [DisplayFormat(DataFormatString = "{0:F1}")] 
        public double TotalScore { get; set; }

        [Display(Name = "Số lượt xem")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ViewCount { get; set; }

        [Display(Name = "Số lượng đã bán")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Sales { get; set; }

        [Display(Name = "Điểm trung bình")]
        [DisplayFormat(DataFormatString = "{0:F1}")] 
        public double AvgScore
        {
            get
            {
                return VoteCount > 0 ? Math.Round(TotalScore / VoteCount, 1) : 0;
            }
        }
    }
}