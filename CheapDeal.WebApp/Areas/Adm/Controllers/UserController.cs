using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AdminModels = CheapDeal.WebApp.Areas.Adm.Models;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class UserController : AdminController
    {
        private ShopDbContext db = new ShopDbContext();
        private UserManager<Account> _userManager;

        public UserManager<Account> UserManager
        {
            get
            {
                return _userManager ?? new UserManager<Account>(new UserStore<Account>(db));
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index(string searchTerm, string role, DateTime? fromDate,
            DateTime? toDate, bool? isActive, string sortBy = "CreatedDate",
            string sortOrder = "desc", int page = 1)
        {
            var model = new UserFilterViewModel
            {
                SearchTerm = searchTerm,
                Role = role,
                FromDate = fromDate,
                ToDate = toDate,
                IsActive = isActive,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Page = page,
                PageSize = 20,
                Users = new List<UserProfile>()
            };

            var query = from account in db.Users
                        join profile in db.UserProfiles on account.Id equals profile.AccountId into profileGroup
                        from p in profileGroup.DefaultIfEmpty()
                        select new
                        {
                            Account = account,
                            Profile = p,
                            AccountId = account.Id,
                            FullName = p != null ? p.FullName : "",
                            Phone = p != null ? p.Phone : "",
                            Address = p != null ? p.Address : "",
                            Birthday = p != null ? p.Birthday : (DateTime?)null,
                            Gender = p != null ? p.Gender : (bool?)null,
                            CreatedDate = p != null ? p.CreatedDate : (DateTime?)null,
                            UpdatedDate = p != null ? p.UpdatedDate : (DateTime?)null
                        };
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x =>
                    x.Account.UserName.Contains(searchTerm) ||
                    x.Account.Email.Contains(searchTerm) ||
                    (x.FullName != null && x.FullName.Contains(searchTerm)) ||
                    (x.Phone != null && x.Phone.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(role))
            {
                var roleId = db.Roles.Where(r => r.Name == role).Select(r => r.Id).FirstOrDefault();
                if (!string.IsNullOrEmpty(roleId))
                {
                    var usersInRole = db.Users
                        .Where(u => u.Roles.Any(r => r.RoleId == roleId))
                        .Select(u => u.Id)
                        .ToList();

                    query = query.Where(x => usersInRole.Contains(x.Account.Id));
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.Account.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.AddDays(1).AddSeconds(-1);
                query = query.Where(x => x.Account.CreatedDate <= toDateEnd);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.Account.IsActive == isActive.Value);
            }
            switch (sortBy)
            {
                case "UserName":
                    query = sortOrder == "asc" ?
                        query.OrderBy(x => x.Account.UserName) :
                        query.OrderByDescending(x => x.Account.UserName);
                    break;
                case "Email":
                    query = sortOrder == "asc" ?
                        query.OrderBy(x => x.Account.Email) :
                        query.OrderByDescending(x => x.Account.Email);
                    break;
                case "FullName":
                    query = sortOrder == "asc" ?
                        query.OrderBy(x => x.FullName) :
                        query.OrderByDescending(x => x.FullName);
                    break;
                default: // CreatedDate
                    query = sortOrder == "asc" ?
                        query.OrderBy(x => x.Account.CreatedDate) :
                        query.OrderByDescending(x => x.Account.CreatedDate);
                    break;
            }

            model.TotalRecords = query.Count();

            var results = query
                .Skip((page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            model.Users = results.Select(x =>
            {
                if (x.Profile != null)
                {
                    x.Profile.Account = x.Account;
                    return x.Profile;
                }
                else
                {
                    return new UserProfile
                    {
                        AccountId = x.AccountId,
                        Account = x.Account,
                        FullName = x.FullName ?? "",
                        Phone = x.Phone ?? "",
                        Address = x.Address ?? "",
                        Birthday = x.Birthday,
                        Gender = x.Gender,
                        CreatedDate = x.CreatedDate ?? DateTime.Now,
                        UpdatedDate = x.UpdatedDate
                    };
                }
            }).ToList();

            ViewBag.Roles = new SelectList(db.Roles.OrderBy(r => r.Name), "Name", "Name", role);

            return View(model);
        }

        // GET: Adm/User/Customer
        public ActionResult Customer(int page = 1)
        {
            return Index(null, "Customer", null, null, null, "CreatedDate", "desc", page);
        }

        // GET: Adm/User/Sale
        public ActionResult Sale(int page = 1)
        {
            return Index(null, "Sale", null, null, null, "CreatedDate", "desc", page);
        }

        // GET: Adm/User/Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Admin(int page = 1)
        {
            return Index(null, "Admin", null, null, null, "CreatedDate", "desc", page);
        }

        // GET: Adm/User/Create
        public ActionResult Create()
        {
            var model = new CreateUserViewModel();
            ViewBag.Roles = new SelectList(db.Roles.OrderBy(r => r.Name), "Name", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Account
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                };

                var result = UserManager.Create(user, model.Password);

                if (result.Succeeded)
                {
                    var profile = new UserProfile
                    {
                        AccountId = user.Id,
                        FullName = model.FullName ?? "",
                        Phone = model.Phone ?? "",
                        Address = model.Address ?? "",
                        Birthday = model.Birthday,
                        Gender = model.Gender,
                        CreatedDate = DateTime.Now,  
                        UpdatedDate = DateTime.Now   
                    };

                    db.UserProfiles.Add(profile);
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        UserManager.AddToRole(user.Id, model.Role);
                    }

                    db.SaveChanges();

                    LogActivity(User.Identity.GetUserId(), "Tạo người dùng",
                        $"Tạo tài khoản: {model.Email}");

                    TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                    return RedirectToAction("Index");
                }

                AddErrors(result);
            }

            ViewBag.Roles = new SelectList(db.Roles.OrderBy(r => r.Name), "Name", "Name", model.Role);
            return View(model);
        }

        // GET: Adm/User/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var profile = db.UserProfiles.FirstOrDefault(p => p.AccountId == id);
            var userRoles = UserManager.GetRoles(id);

            var model = new EditUserViewModel
            {
                AccountId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = profile?.FullName ?? "",
                Phone = profile?.Phone ?? "",
                Address = profile?.Address ?? "",
                Birthday = profile?.Birthday,
                Gender = profile?.Gender,
                IsActive = user.IsActive,
                Role = userRoles.FirstOrDefault()
            };

            ViewBag.Roles = new SelectList(db.Roles.OrderBy(r => r.Name), "Name", "Name", model.Role);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(model.AccountId);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.Email = model.Email;
                user.IsActive = model.IsActive;

                var profile = db.UserProfiles.FirstOrDefault(p => p.AccountId == model.AccountId);
                if (profile == null)
                {
                    profile = new UserProfile
                    {
                        AccountId = model.AccountId,
                        CreatedDate = DateTime.Now
                    };
                    db.UserProfiles.Add(profile);
                }

                profile.FullName = model.FullName ?? "";
                profile.Phone = model.Phone ?? "";
                profile.Address = model.Address ?? "";
                profile.Birthday = model.Birthday;
                profile.Gender = model.Gender;
                profile.UpdatedDate = DateTime.Now;

                // Update Role
                var currentRoles = UserManager.GetRoles(model.AccountId);
                if (currentRoles.Count > 0)
                {
                    UserManager.RemoveFromRoles(model.AccountId, currentRoles.ToArray());
                }
                if (!string.IsNullOrEmpty(model.Role))
                {
                    UserManager.AddToRole(model.AccountId, model.Role);
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                LogActivity(User.Identity.GetUserId(), "Cập nhật người dùng",
                    $"Cập nhật: {model.Email}");

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = new SelectList(db.Roles.OrderBy(r => r.Name), "Name", "Name", model.Role);
            return View(model);
        }

        //  GET: Adm/User/Details/5 
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var profile = db.UserProfiles.FirstOrDefault(p => p.AccountId == id);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    AccountId = id,
                    FullName = "",
                    Phone = "",
                    Address = "",
                    Birthday = null,
                    Gender = null
                };
            }

            var roles = UserManager.GetRoles(id);
            var orderCount = db.Orders.Count(o => o.CustomerId == id);
            var totalSpent = db.Orders.Where(o => o.CustomerId == id).Sum(o => (decimal?)o.TotalPrice) ?? 0;

            var recentActivities = db.UserActivityLogs
                .Where(a => a.AccountId == id)
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            var model = new UserDetailsViewModel
            {
                Account = user,
                Profile = profile,
                Roles = roles.ToList(),
                OrderCount = orderCount,
                TotalSpent = totalSpent,
                RecentActivities = recentActivities
            };

            return View(model);
        }

        // GET: Adm/User/Roles
        public ActionResult Roles(string role, int page = 1)
        {
            var pageSize = 20;
            var users = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                var roleId = db.Roles.Where(r => r.Name == role).Select(r => r.Id).FirstOrDefault();
                if (!string.IsNullOrEmpty(roleId))
                {
                    users = users.Where(u => u.Roles.Any(r => r.RoleId == roleId));
                }
            }

            var totalRecords = users.Count();
            var userList = users
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = userList.Select(u => new UserRoleViewModel
            {
                AccountId = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = UserManager.GetRoles(u.Id).ToList()
            }).ToList();

            ViewBag.Role = role;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.Roles = new SelectList(db.Roles.OrderBy(r => r.Name), "Name", "Name", role);

            return View(model);
        }

        // GET: Adm/User/ManageRoles/5
        public ActionResult ManageRoles(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = UserManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = UserManager.GetRoles(id);

            var model = new ManageUserRolesViewModel
            {
                AccountId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = new List<RoleAssignment>()
            };

            var allRoles = db.Roles.ToList();
            foreach (var role in allRoles)
            {
                model.Roles.Add(new RoleAssignment
                {
                    RoleName = role.Name,
                    RoleDisplayName = GetRoleDisplayName(role.Name),
                    IsAssigned = userRoles.Contains(role.Name)
                });
            }

            return View(model);
        }

        // POST: Adm/User/ManageRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageRoles(ManageUserRolesViewModel model)
        {
            var user = UserManager.FindById(model.AccountId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var currentRoles = UserManager.GetRoles(user.Id);
            UserManager.RemoveFromRoles(user.Id, currentRoles.ToArray());

            var selectedRoles = model.Roles.Where(r => r.IsAssigned).Select(r => r.RoleName).ToArray();
            if (selectedRoles.Length > 0)
            {
                UserManager.AddToRoles(user.Id, selectedRoles);
            }

            LogActivity(User.Identity.GetUserId(), "Phân quyền",
                $"Cập nhật quyền cho: {user.Email}");

            TempData["SuccessMessage"] = "Cập nhật phân quyền thành công!";
            return RedirectToAction("Roles");
        }

        // GET: Adm/User/Activity
        public ActionResult Activity(string accountId, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            var pageSize = 50;
            var query = db.UserActivityLogs.Include(a => a.Account).AsQueryable();

            if (!string.IsNullOrEmpty(accountId))
            {
                query = query.Where(a => a.AccountId == accountId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.AddDays(1).AddSeconds(-1);
                query = query.Where(a => a.Timestamp <= toDateEnd);
            }

            query = query.OrderByDescending(a => a.Timestamp);

            var totalRecords = query.Count();
            var activities = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.AccountId = accountId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.Users = new SelectList(db.Users.OrderBy(u => u.UserName), "Id", "UserName", accountId);

            return View(activities);
        }

        private void LogActivity(string accountId, string activity, string description = null)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                    return;

                var log = new UserActivityLog
                {
                    AccountId = accountId,
                    ActivityType = activity,
                    Description = description,
                    Timestamp = DateTime.Now,
                    IpAddress = Request.UserHostAddress,
                    UserAgent = Request.UserAgent
                };

                db.UserActivityLogs.Add(log);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging activity: {ex.Message}");
            }
        }

        // GET: Adm/User/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var currentUserId = User.Identity.GetUserId();
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không thể xóa chính tài khoản của mình!";
                return RedirectToAction("Index");
            }

            var profile = db.UserProfiles.FirstOrDefault(p => p.AccountId == id);
            var userRoles = UserManager.GetRoles(id);

            var model = new UserProfile
            {
                AccountId = user.Id,
                Account = user,
                FullName = profile?.FullName ?? "",
                Phone = profile?.Phone ?? "",
                Address = profile?.Address ?? "",
                Birthday = profile?.Birthday,
                Gender = profile?.Gender
            };

            ViewBag.UserRoles = string.Join(", ", userRoles.Select(r => GetRoleDisplayName(r)));

            return View(model);
        }

        // POST: Adm/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var currentUserId = User.Identity.GetUserId();
                if (id == currentUserId)
                {
                    TempData["ErrorMessage"] = "Bạn không thể xóa chính tài khoản của mình!";
                    return RedirectToAction("Index");
                }
                var profile = db.UserProfiles.FirstOrDefault(p => p.AccountId == id);
                if (profile != null)
                {
                    db.UserProfiles.Remove(profile);
                }

                var activityLogs = db.UserActivityLogs.Where(a => a.AccountId == id);
                db.UserActivityLogs.RemoveRange(activityLogs);
                db.Users.Remove(user);
                db.SaveChanges();

                LogActivity(User.Identity.GetUserId(), "Xóa người dùng",
                    $"Đã xóa tài khoản: {user.Email}");

                TempData["SuccessMessage"] = "Xóa người dùng thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng. Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Adm/User/ToggleStatus
        [HttpPost]
        public JsonResult ToggleStatus(string id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                user.IsActive = !user.IsActive;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                LogActivity(User.Identity.GetUserId(),
                    user.IsActive ? "Kích hoạt tài khoản" : "Khóa tài khoản",
                    $"Tài khoản: {user.Email}");

                return Json(new
                {
                    success = true,
                    isActive = user.IsActive,
                    message = user.IsActive ? "Đã kích hoạt tài khoản" : "Đã khóa tài khoản"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private string GetRoleDisplayName(string roleName)
        {
            switch (roleName)
            {
                case "Admin":
                    return "Quản trị viên";
                case "Manager":
                    return "Quản lý";
                case "Sale":
                    return "Nhân viên bán hàng";
                case "Customer":
                    return "Khách hàng";
                default:
                    return roleName;
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
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}