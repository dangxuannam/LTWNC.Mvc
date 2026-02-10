using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    [Authorize]
    public class ProfileController : AdminController
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Adm/Profile/Index 
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var account = db.Users
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.Id == userId);

            if (account == null)
            {
                return HttpNotFound();
            }

            var profile = account.Profile;
            if (profile == null)
            {
                profile = new UserProfile
                {
                    AccountId = userId,
                    CreatedDate = DateTime.Now
                };
            }
            ViewBag.AccountId = userId;
            ViewBag.Email = account.Email;
            ViewBag.UserName = account.UserName;
            ViewBag.Phone = account.PhoneNumber;
            ViewBag.FullName = profile.FullName;
            ViewBag.Address = profile.Address;
            ViewBag.Birthday = profile.Birthday;
            ViewBag.Gender = profile.Gender;
            ViewBag.AvatarUrl = profile.AvatarUrl;

            return View(profile);
        }

        // POST: Adm/Profile/Edit 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Account model, HttpPostedFileBase avatarFile)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users
                    .Include(u => u.Profile)
                    .FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    if (user.Profile != null)
                    {
                        user.Profile.FullName = model.Profile.FullName;
                        user.Profile.Birthday = model.Profile.Birthday;
                        user.Profile.Gender = model.Profile.Gender;
                        user.Profile.Address = model.Profile.Address;
                        user.Profile.UpdatedDate = DateTime.Now;

                        if (avatarFile != null && avatarFile.ContentLength > 0)
                        {

                            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                            string fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                TempData["Error"] = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)";
                                return View(model);
                            }
                            if (avatarFile.ContentLength > 2 * 1024 * 1024)
                            {
                                TempData["Error"] = "Kích thước file không được vượt quá 2MB";
                                return View(model);
                            }

                            var fileName = Path.GetFileName(avatarFile.FileName);
                            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            var uploadFolder = Server.MapPath("~/Images/");
                            if (!Directory.Exists(uploadFolder))
                            {
                                Directory.CreateDirectory(uploadFolder);
                            }

                            var path = Path.Combine(uploadFolder, uniqueFileName);
                            if (!string.IsNullOrEmpty(user.Profile.AvatarUrl))
                            {
                                string oldFilePath = Server.MapPath(user.Profile.AvatarUrl);
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }
                            }

                            avatarFile.SaveAs(path);
                            user.Profile.AvatarUrl = "/Images/" + uniqueFileName;
                        }
                    }
                    else
                    {
                        user.Profile = new UserProfile
                        {
                            AccountId = userId,
                            FullName = model.Profile.FullName,
                            Birthday = model.Profile.Birthday,
                            Gender = model.Profile.Gender,
                            Address = model.Profile.Address,
                            CreatedDate = DateTime.Now
                        };
                        db.UserProfiles.Add(user.Profile);
                    }

                    user.PhoneNumber = model.PhoneNumber;
                    user.Email = model.Email;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Adm/Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.Identity.GetUserId();
                var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }

                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Index");
                }

                AddErrors(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }

            return View(model);
        }

        // GET: Adm/Profile/Messages 
        public ActionResult Messages()
        {
            var userId = User.Identity.GetUserId();

            // Lấy danh sách tin nhắn đến
            var messages = db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ReceiverId == userId && !m.IsDeletedByReceiver)
                .OrderByDescending(m => m.SentDate)
                .ToList();

            ViewBag.CurrentUserId = userId;
            ViewBag.UnreadCount = messages.Count(m => !m.IsRead);

            return View(messages);
        }

        [HttpPost]
        public JsonResult MarkAsRead(int messageId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var message = db.Messages.Find(messageId);

                if (message != null && message.ReceiverId == userId)
                {
                    message.IsRead = true;
                    message.ReadDate = DateTime.Now;
                    db.Entry(message).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Không tìm thấy tin nhắn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Adm/Profile/DeleteMessage
        [HttpPost]
        public JsonResult DeleteMessage(int messageId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var message = db.Messages.Find(messageId);

                if (message != null)
                {
                    if (message.SenderId == userId)
                    {
                        message.IsDeletedBySender = true;
                    }
                    else if (message.ReceiverId == userId)
                    {
                        message.IsDeletedByReceiver = true;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không có quyền xóa" });
                    }

                    if (message.IsDeletedBySender && message.IsDeletedByReceiver)
                    {
                        db.Messages.Remove(message);
                    }
                    else
                    {
                        db.Entry(message).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Không tìm thấy tin nhắn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public JsonResult GetUnreadCount()
        {
            var userId = User.Identity.GetUserId();
            var count = db.Messages
                .Count(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeletedByReceiver);

            return Json(new { success = true, count = count }, JsonRequestBehavior.AllowGet);
        }

        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;

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

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
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

                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}