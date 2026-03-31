using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
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
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            System.Diagnostics.Debug.WriteLine("[LOGIN] POST action started");
            System.Diagnostics.Debug.WriteLine($"[LOGIN] Username: {model.UserName}, Remember: {model.RememberMe}");
            
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] ✗ ModelState is invalid");
                return View(model);
            }
            
            System.Diagnostics.Debug.WriteLine("[LOGIN] Attempting password sign in...");
            var result = await SignInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                shouldLockout: true);
            
            System.Diagnostics.Debug.WriteLine($"[LOGIN] Sign-in result: {result}");
            
            switch (result)
            {
                case SignInStatus.Success:
                    System.Diagnostics.Debug.WriteLine("[LOGIN] ✓ Sign-in successful");

                    var user = UserManager.FindByName(model.UserName);
                    if (user != null)
                    {
                        var roles = UserManager.GetRoles(user.Id);
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] User roles: {string.Join(", ", roles)}");

                        if (roles.Contains("Admin") || roles.Contains("Manager") || roles.Contains("Sale"))
                        {
                            System.Diagnostics.Debug.WriteLine("[LOGIN] Redirecting to Admin Dashboard");
                            return RedirectToAction("Index", "Dashboard", new { area = "Adm" });
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Redirecting to return URL: {returnUrl}");
                        return Redirect(returnUrl);
                    }
                    
                    System.Diagnostics.Debug.WriteLine("[LOGIN] Redirecting to Home");
                    return RedirectToAction("Index", "Home");

                case SignInStatus.LockedOut:
                    System.Diagnostics.Debug.WriteLine("[LOGIN] ✗ Account is locked out");
                    return View("Lockout");
                    
                case SignInStatus.RequiresVerification:
                    System.Diagnostics.Debug.WriteLine("[LOGIN] Requires verification (2FA)");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    
                case SignInStatus.Failure:
                default:
                    System.Diagnostics.Debug.WriteLine("[LOGIN] ✗ Invalid login attempt");
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("[REGISTER] POST action started");
            
            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"[REGISTER] Creating user with UserName: {model.UserName}, Email: {model.Email}");
                
                var user = new Account { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    System.Diagnostics.Debug.WriteLine($"[REGISTER] ✓ User created successfully. UserId: {user.Id}");
                    System.Diagnostics.Debug.WriteLine($"[REGISTER] Attempting to sign in user...");
                    
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    
                    System.Diagnostics.Debug.WriteLine($"[REGISTER] ✓ User signed in successfully");
                    return RedirectToAction("Index", "Home");
                }
                
                System.Diagnostics.Debug.WriteLine($"[REGISTER] ✗ User creation failed. Error count: {result.Errors.Count()}");
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"[REGISTER] Error: {error}");
                }
                AddErrors(result);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[REGISTER] ✗ ModelState is invalid");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"[REGISTER] Validation Error: {error.ErrorMessage}");
                    }
                }
            }

            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Tìm user theo EMAIL (không phải username)
            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // Email không tồn tại → hiện thông báo rõ ràng (không ẩn)
                ModelState.AddModelError("Email", "Email này chưa được đăng ký trong hệ thống.");
                return View(model);
            }

            // Tạo OTP 6 chữ số
            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(10);

            // Lưu OTP vào Session (dùng email làm key vì user chưa đăng nhập)
            Session["ForgotPassOTP_" + user.Id] = otp;
            Session["ForgotPassOTP_Expiry_" + user.Id] = expiry;
            Session["ForgotPassUserId"] = user.Id; // để dùng ở bước VerifyOtp

            // Gửi email OTP
            try
            {
                await UserManager.SendEmailAsync(user.Id,
                    "Mã OTP khôi phục mật khẩu",
                    $@"<div style='font-family:Arial,sans-serif;max-width:500px;margin:auto;border:1px solid #e0e0e0;border-radius:12px;overflow:hidden;'>
                        <div style='background:linear-gradient(135deg,#17a2b8,#20c997);padding:30px;text-align:center;'>
                            <h2 style='color:white;margin:0;'>🔐 Khôi phục mật khẩu</h2>
                        </div>
                        <div style='padding:30px;background:#fff;'>
                            <p style='color:#333;'>Xin chào <strong>{user.UserName}</strong>,</p>
                            <p style='color:#555;'>Bạn đã yêu cầu đặt lại mật khẩu. Đây là mã OTP của bạn:</p>
                            <div style='background:#f0f9ff;border:2px dashed #17a2b8;border-radius:10px;padding:20px;text-align:center;margin:20px 0;'>
                                <span style='font-size:36px;font-weight:bold;letter-spacing:8px;color:#17a2b8;'>{otp}</span>
                            </div>
                            <p style='color:#e74c3c;font-size:13px;'>⚠ Mã OTP có hiệu lực trong <strong>10 phút</strong>. Không chia sẻ mã này cho bất kỳ ai.</p>
                            <p style='color:#95a5a6;font-size:12px;'>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.</p>
                        </div>
                    </div>"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendOTP ForgotPassword Error: " + ex.Message);
                ModelState.AddModelError("", "Gửi email thất bại. Vui lòng kiểm tra cấu hình SMTP.");
                return View(model);
            }

            // Ẩn email một phần để hiển thị: abc***@gmail.com
            var parts = model.Email.Split('@');
            var masked = parts[0].Substring(0, Math.Min(3, parts[0].Length)) + "***@" + parts[1];
            TempData["MaskedEmail"] = masked;

            return RedirectToAction("VerifyOtp");
        }

        // GET: /Account/VerifyOtp
        [AllowAnonymous]
        public ActionResult VerifyOtp()
        {
            if (Session["ForgotPassUserId"] == null)
                return RedirectToAction("ForgotPassword");

            return View(new VerifyOtpViewModel
            {
                Email = TempData["MaskedEmail"] as string ?? ""
            });
        }

        // POST: /Account/VerifyOtp
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            var userId = Session["ForgotPassUserId"] as string;
            if (userId == null)
                return RedirectToAction("ForgotPassword");

            if (!ModelState.IsValid)
                return View(model);

            var savedOtp = Session["ForgotPassOTP_" + userId] as string;
            var expiry = Session["ForgotPassOTP_Expiry_" + userId] as DateTime?;

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

            // OTP đúng → tạo reset token và chuyển sang trang đặt mật khẩu mới
            var resetToken = await UserManager.GeneratePasswordResetTokenAsync(userId);

            // Xóa OTP khỏi session
            Session.Remove("ForgotPassOTP_" + userId);
            Session.Remove("ForgotPassOTP_Expiry_" + userId);
            // Lưu token và userId để dùng ở ResetPassword
            Session["ForgotPassResetToken"] = resetToken;
            // Giữ ForgotPassUserId để ResetPassword dùng

            return RedirectToAction("ResetPassword");
        }

        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            // Cho phép vào nếu có reset token trong session (flow OTP)
            if (Session["ForgotPassResetToken"] != null && Session["ForgotPassUserId"] != null)
                return View(new ResetPasswordViewModel());

            return View("Error");
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("[RESET_PASSWORD] POST action started");
            
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("[RESET_PASSWORD] ✗ ModelState is INVALID");
                System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] Total errors: {ModelState.Values.Sum(v => v.Errors.Count)}");
                
                // Log từng lỗi
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] ERROR: {error.ErrorMessage}");
                        if (error.Exception != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] EXCEPTION: {error.Exception.Message}");
                        }
                    }
                }
                
                return View(model);
            }

            System.Diagnostics.Debug.WriteLine("[RESET_PASSWORD] ✓ ModelState is valid");

            var userId = Session["ForgotPassUserId"] as string;
            var resetToken = Session["ForgotPassResetToken"] as string;

            System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] userId: {(string.IsNullOrEmpty(userId) ? "NULL" : userId)}");
            System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] resetToken: {(string.IsNullOrEmpty(resetToken) ? "NULL" : "EXISTS")}");

            if (userId == null || resetToken == null)
            {
                System.Diagnostics.Debug.WriteLine("[RESET_PASSWORD] ✗ Session expired - userId or resetToken is null");
                ModelState.AddModelError("", "Phiên làm việc đã hết hạn. Vui lòng thực hiện lại.");
                return View(model);
            }

            System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] Attempting to reset password for user: {userId}");
            System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] New password length: {model.Password?.Length ?? 0}");

            var result = await UserManager.ResetPasswordAsync(userId, resetToken, model.Password);
            
            if (result.Succeeded)
            {
                System.Diagnostics.Debug.WriteLine("[RESET_PASSWORD] ✓ Password reset successful");
                
                // Dọn sạch session
                Session.Remove("ForgotPassUserId");
                Session.Remove("ForgotPassResetToken");

                return RedirectToAction("ResetPasswordConfirmation");
            }

            System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] ✗ Password reset failed. Error count: {result.Errors.Count()}");
            foreach (var error in result.Errors)
            {
                System.Diagnostics.Debug.WriteLine($"[RESET_PASSWORD] Error: {error}");
            }

            AddErrors(result);
            return View(model);
        }

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ForgotPasswordConfirmation (giữ lại nếu có view cũ dùng)
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:    
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new Account { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            TempData["SuccessMessage"] = "Đã đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            TempData["SuccessMessage"] = "Đã đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOffAdmin()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            TempData["SuccessMessage"] = "Đã đăng xuất khỏi hệ thống quản trị!";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}