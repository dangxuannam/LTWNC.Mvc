using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CheapDeal.WebApp.Models;

namespace CheapDeal.WebApp.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController() { }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Đổi mật khẩu thành công."
                : message == ManageMessageId.SetPasswordSuccess ? "Đặt mật khẩu thành công."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Đã cập nhật xác thực hai yếu tố."
                : message == ManageMessageId.Error ? "Có lỗi xảy ra."
                : message == ManageMessageId.AddPhoneSuccess ? "Đã thêm số điện thoại."
                : message == ManageMessageId.RemovePhoneSuccess ? "Đã xóa số điện thoại."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Mã xác thực của bạn là: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Index", "Manage");
        }

        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Index", "Manage");
        }

        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            ModelState.AddModelError("", "Xác thực số điện thoại thất bại.");
            return View(model);
        }

        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        // =====================================================
        // ĐỔI MẬT KHẨU CÓ OTP
        // =====================================================

        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Manage/SendChangePasswordOtp
        // Gửi OTP về email của user đang đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendChangePasswordOtp()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            if (string.IsNullOrEmpty(user.Email))
                return Json(new { success = false, message = "Tài khoản chưa có email. Vui lòng cập nhật email trước." });

            // Tạo OTP 6 số
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu vào Session 10 phút
            Session["ChangePassOTP_" + user.Id] = otp;
            Session["ChangePassOTP_Expiry_" + user.Id] = DateTime.Now.AddMinutes(10);

            try
            {
                await UserManager.SendEmailAsync(
                    user.Id,
                    "Mã OTP xác nhận đổi mật khẩu - WebTaiSan",
                    $@"<div style='font-family:Arial,sans-serif;max-width:500px;margin:auto;padding:30px;
                               border:1px solid #e1e8ed;border-radius:12px;'>
                        <div style='text-align:center;background:#20c997;padding:24px;border-radius:8px 8px 0 0;'>
                            <h2 style='color:white;margin:0;'>🔑 Xác nhận đổi mật khẩu</h2>
                        </div>
                        <div style='padding:28px 24px;'>
                            <p style='color:#2c3e50;'>Xin chào <strong>{user.UserName}</strong>,</p>
                            <p style='color:#5a6c7d;'>Bạn vừa yêu cầu đổi mật khẩu trên WebTaiSan. Đây là mã OTP xác nhận:</p>
                            <div style='background:#f0fdf4;border:2px dashed #20c997;border-radius:10px;
                                        text-align:center;padding:24px;margin:24px 0;'>
                                <span style='font-size:42px;font-weight:bold;letter-spacing:14px;color:#20c997;'>{otp}</span>
                            </div>
                            <p style='color:#e74c3c;font-size:13px;'>
                                ⚠ Mã OTP có hiệu lực trong <strong>10 phút</strong>.
                                Không chia sẻ mã này cho bất kỳ ai.
                            </p>
                            <p style='color:#95a5a6;font-size:12px;'>
                                Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.
                            </p>
                        </div>
                    </div>"
                );

                // Ẩn một phần email để bảo mật: abc***@gmail.com
                var emailParts = user.Email.Split('@');
                var maskedEmail = emailParts[0].Substring(0, Math.Min(3, emailParts[0].Length))
                                + "***@" + emailParts[1];

                return Json(new { success = true, message = "Mã OTP đã được gửi đến " + maskedEmail });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendOTP Error: " + ex.Message);
                return Json(new { success = false, message = "Gửi email thất bại. Vui lòng kiểm tra cấu hình SMTP trong Web.config." });
            }
        }

        // POST: /Manage/ChangePassword
        // Xác thực OTP + đổi mật khẩu mới (không cần mật khẩu cũ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordWithOtpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản.");
                return View(model);
            }

            // Kiểm tra OTP từ Session
            var savedOtp = Session["ChangePassOTP_" + user.Id] as string;
            var expiry = Session["ChangePassOTP_Expiry_" + user.Id] as DateTime?;

            if (expiry == null || DateTime.Now > expiry.Value)
            {
                ModelState.AddModelError("Otp", "Mã OTP đã hết hạn. Vui lòng gửi lại.");
                return View(model);
            }

            if (string.IsNullOrEmpty(savedOtp) || savedOtp != model.Otp.Trim())
            {
                ModelState.AddModelError("Otp", "Mã OTP không chính xác.");
                return View(model);
            }

            // Xóa OTP sau khi xác thực thành công
            Session.Remove("ChangePassOTP_" + user.Id);
            Session.Remove("ChangePassOTP_Expiry_" + user.Id);

            // Đổi mật khẩu bằng reset token (không cần mật khẩu cũ)
            var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var result = await UserManager.ResetPasswordAsync(user.Id, resetToken, model.NewPassword);

            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }

            AddErrors(result);
            return View(model);
        }

        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Đã xóa đăng nhập ngoài."
                : message == ManageMessageId.Error ? "Có lỗi xảy ra."
                : "";

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
                return View("Error");

            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes()
                .Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            return new AccountController.ChallengeResult(provider,
                Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });

            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded
                ? RedirectToAction("ManageLogins")
                : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            base.Dispose(disposing);
        }

        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return user?.PasswordHash != null;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return user?.PhoneNumber != null;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
    }
}