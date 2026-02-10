using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using CheapDeal.WebApp.Models;
using CheapDeal.WebApp.DAL;

namespace CheapDeal.WebApp
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            try
            {
                // Lấy cấu hình SMTP từ Web.config
                var smtpHost = ConfigurationManager.AppSettings["SMTP:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTP:Port"] ?? "587");
                var smtpUsername = ConfigurationManager.AppSettings["SMTP:Username"];
                var smtpPassword = ConfigurationManager.AppSettings["SMTP:Password"];
                var smtpFromEmail = ConfigurationManager.AppSettings["SMTP:FromEmail"] ?? smtpUsername;
                var smtpFromName = ConfigurationManager.AppSettings["SMTP:FromName"] ?? "WebTaiSan";
                var smtpEnableSSL = bool.Parse(ConfigurationManager.AppSettings["SMTP:EnableSSL"] ?? "true");

                // Kiểm tra cấu hình
                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    System.Diagnostics.Debug.WriteLine("SMTP not configured. Email not sent.");
                    return;
                }

                // Tạo email message
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpFromEmail, smtpFromName),
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(message.Destination);

                // Cấu hình SMTP client
                using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = smtpEnableSSL;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;

                    // Gửi email
                    await smtpClient.SendMailAsync(mailMessage);
                    System.Diagnostics.Debug.WriteLine($"Email sent to {message.Destination}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
                // Log error nhưng không throw exception để không ảnh hưởng flow
            }
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<Account>
    {
        public ApplicationUserManager(IUserStore<Account> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<Account>(context.Get<ShopDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<Account>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,  // Cho phép không cần ký tự đặc biệt
                RequireDigit = false,              // Cho phép không cần số
                RequireLowercase = false,          // Cho phép không cần chữ thường
                RequireUppercase = false,          // Cho phép không cần chữ hoa
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<Account>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<Account>
            {
                Subject = "Mã bảo mật",
                BodyFormat = "Mã bảo mật của bạn là: {0}"
            });

            // Cấu hình Email Service
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<Account>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<Account, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(Account user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}