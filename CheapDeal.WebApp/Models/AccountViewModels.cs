using System;
using System.ComponentModel.DataAnnotations;

namespace CheapDeal.WebApp.Models
{
    public class ProfileViewModel
    {
        public Account Account { get; set; }
        public UserProfile Profile { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(200)]
        public string Address { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }

        [Display(Name = "Giới tính")]
        public bool? Gender { get; set; }
    }

    public class SecurityViewModel
    {
        [Display(Name = "Xác thực hai yếu tố")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "Số điện thoại đã xác nhận")]
        public bool PhoneNumberConfirmed { get; set; }

        [Display(Name = "Email đã xác nhận")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Khóa tài khoản")]
        public bool LockoutEnabled { get; set; }
    }

}