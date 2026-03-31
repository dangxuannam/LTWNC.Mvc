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
using CheapDeal.WebApp.Helpers;

namespace CheapDeal.WebApp
{
    public class EmailService : IIdentityMessageService
    {
        internal static void SendPendingReminders()
        {
            throw new NotImplementedException();
        }

        internal static bool SendTestEmail(string testEmail, out string error)
        {
            throw new NotImplementedException();
        }

        public async Task SendAsync(IdentityMessage message)
        {
            LogHelper.LogEmail("========== EMAIL SERVICE CALLED ==========");
            try
            {
                LogHelper.LogEmail("[EMAIL] Step 1: Reading SMTP configuration from Web.config");
                
                // Lấy cấu hình SMTP từ Web.config
                var smtpHost = ConfigurationManager.AppSettings["Smtp.Host"] ?? "smtp.gmail.com";
                var smtpPortStr = ConfigurationManager.AppSettings["Smtp.Port"] ?? "587";
                var smtpUsername = ConfigurationManager.AppSettings["Smtp.User"];
                var smtpPassword = ConfigurationManager.AppSettings["Smtp.Password"];
                var smtpFromEmail = ConfigurationManager.AppSettings["Smtp.From"] ?? smtpUsername;
                var smtpFromName = ConfigurationManager.AppSettings["Smtp.FromName"] ?? "CheapDeal System";
                var smtpEnableSslStr = ConfigurationManager.AppSettings["Smtp.EnableSSL"] ?? "true";

                LogHelper.LogEmail($"[EMAIL] Config loaded - Host: {smtpHost}, Port: {smtpPortStr}");
                
                int smtpPort;
                if (!int.TryParse(smtpPortStr, out smtpPort))
                {
                    LogHelper.LogEmail($"[EMAIL] ERROR: Invalid SMTP Port value: {smtpPortStr}");
                    return;
                }

                bool smtpEnableSSL;
                if (!bool.TryParse(smtpEnableSslStr, out smtpEnableSSL))
                {
                    smtpEnableSSL = true;
                }

                LogHelper.LogEmail($"[EMAIL] Step 2: Validating SMTP configuration");
                LogHelper.LogEmail($"[EMAIL] - Host: {smtpHost}");
                LogHelper.LogEmail($"[EMAIL] - Port: {smtpPort}");
                LogHelper.LogEmail($"[EMAIL] - Username: {(string.IsNullOrEmpty(smtpUsername) ? "EMPTY" : "***")}");
                LogHelper.LogEmail($"[EMAIL] - Password: {(string.IsNullOrEmpty(smtpPassword) ? "EMPTY" : "***")}");
                LogHelper.LogEmail($"[EMAIL] - From Email: {smtpFromEmail}");
                LogHelper.LogEmail($"[EMAIL] - SSL Enabled: {smtpEnableSSL}");

                // Kiểm tra cấu hình
                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    LogHelper.LogEmail("[EMAIL] ✗ BLOCKED: SMTP Username or Password is EMPTY in Web.config");
                    return;
                }

                LogHelper.LogEmail($"[EMAIL] Step 3: Validating email message");
                LogHelper.LogEmail($"[EMAIL] - Destination: {message.Destination}");
                LogHelper.LogEmail($"[EMAIL] - Subject: {message.Subject}");

                if (string.IsNullOrEmpty(message.Destination))
                {
                    LogHelper.LogEmail("[EMAIL] ✗ BLOCKED: Email destination is EMPTY");
                    return;
                }

                LogHelper.LogEmail($"[EMAIL] Step 4: Creating mail message");
                
                // Tạo email message
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpFromEmail, smtpFromName);
                    mailMessage.To.Add(message.Destination);
                    mailMessage.Subject = message.Subject;
                    mailMessage.Body = message.Body;
                    mailMessage.IsBodyHtml = true;

                LogHelper.LogEmail($"[EMAIL] Step 5: Connecting to SMTP server {smtpHost}:{smtpPort}");
                    
                    // Cấu hình SMTP client với timeout
                    using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                    {
                        // QUAN TRỌNG: Phải set EnableSsl TRƯỚC khi set Credentials cho Gmail
                        smtpClient.EnableSsl = smtpEnableSSL;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Timeout = 30000; // 30 seconds timeout
                        
                        // Sau đó mới set credentials
                        smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                        LogHelper.LogEmail($"[EMAIL] Step 6: Sending email...");
                        
                        // Gửi email
                        await smtpClient.SendMailAsync(mailMessage);
                        LogHelper.LogEmail($"[EMAIL] ✓ SUCCESS: Email sent to {message.Destination}");
                    }
                }
            }
            catch (System.Net.Mail.SmtpFailedRecipientException failedEx)
            {
                LogHelper.LogEmail($"[EMAIL] ✗ FAILED RECIPIENT: {failedEx.FailedRecipient}");
                LogHelper.LogEmail($"[EMAIL] Message: {failedEx.Message}");
                LogHelper.LogEmail($"[EMAIL] Stack: {failedEx.StackTrace}");
            }
            catch (SmtpException smtpEx)
            {
                LogHelper.LogEmail($"[EMAIL] ✗ SMTP ERROR Code: {smtpEx.StatusCode}");
                LogHelper.LogEmail($"[EMAIL] Message: {smtpEx.Message}");
                LogHelper.LogEmail($"[EMAIL] Inner Exception: {smtpEx.InnerException?.Message}");
                LogHelper.LogEmail($"[EMAIL] Stack: {smtpEx.StackTrace}");
            }
            catch (Exception ex)
            {
                LogHelper.LogEmail($"[EMAIL] ✗ UNEXPECTED ERROR: {ex.GetType().Name}");
                LogHelper.LogEmail($"[EMAIL] Message: {ex.Message}");
                LogHelper.LogEmail($"[EMAIL] Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                LogHelper.LogEmail("========== EMAIL SERVICE FINISHED ==========");
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

    public class ApplicationUserManager : UserManager<Account>
    {
        public ApplicationUserManager(IUserStore<Account> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<Account>(context.Get<ShopDbContext>()));

         
            manager.UserValidator = new UserValidator<Account>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,  
                RequireDigit = false,              
                RequireLowercase = false,          
                RequireUppercase = false,          
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