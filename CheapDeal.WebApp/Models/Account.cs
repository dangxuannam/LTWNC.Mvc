using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CheapDeal.WebApp.Models
{
    public class Account : IdentityUser
    {
        public virtual UserProfile Profile { get; set; }

        public virtual IList<Order> CustomerOrders { get; set; }
        public virtual IList<Order> HandleOrders { get; set; }
      
        public virtual IList<ProductHistory> ProductHistories { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public Account()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Account> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}