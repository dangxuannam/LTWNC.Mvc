using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.Areas.Adm.Models
{
    public class Userviewmodels
    {
        public Account Account { get; set; }
        public UserProfile Profile { get; set; }
    }
    public class UserChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu cũ")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
    public class UserRoleViewModel
    {
        public string AccountId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
    public class ManageUserRolesViewModel
    {
        public string AccountId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<RoleAssignment> Roles { get; set; }
    }

    public class RoleAssignment
    {
        public string RoleName { get; set; }
        public string RoleDisplayName { get; set; }
        public bool IsAssigned { get; set; }
    }
}