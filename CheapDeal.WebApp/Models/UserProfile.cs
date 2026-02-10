using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Models
{
    [Table("UserProfiles")]
    public class UserProfile
    {
        [Key]
        [ForeignKey("Account")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string AccountId { get; set; }

        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? Birthday { get; set; }

        
        [Display(Name = "Giới tính")]
        public bool? Gender { get; set; }

        [StringLength(500)]
        [Display(Name = "Ảnh đại diện")]
        public string AvatarUrl { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        public virtual Account Account { get; set; }
    }
}
