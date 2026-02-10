using CheapDeal.WebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheapDeal.WebApp.DAL
{
    public class AccountSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            var userManager = new UserManager<Account>(new UserStore<Account>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            const string adminRole = "Admin",
                 managerRole = "Manager",
                 saleRole = "Sale",
                 customerRole = "Customer",
                 userName = "xuannam",
                 password = "123456",
                 email = "dangxuannam12a8ts2022@gmail.com";

            if (!roleManager.RoleExists(adminRole))
                roleManager.Create(new IdentityRole(adminRole));

            if (!roleManager.RoleExists(managerRole))
                roleManager.Create(new IdentityRole(managerRole));

            if (!roleManager.RoleExists(saleRole))
                roleManager.Create(new IdentityRole(saleRole));

            if (!roleManager.RoleExists(customerRole))
                roleManager.Create(new IdentityRole(customerRole));


            if (userManager.FindByName(userName) == null)
            {
                var adminUser = new Account()
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = "0361836189",
                    Profile = new UserProfile()
                    {
                        FullName = "Đặng Xuân Nam",
                        Address = "120 Yên Lãng, Hà Nội",
                        Birthday = new DateTime(2004, 09, 23),
                        Gender = true,
                        AvatarUrl = "/images/profile_sm.jpg",
                        CreatedDate = DateTime.Now
                    }
                };

                var result = userManager.Create(adminUser, password);

                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, adminRole);
                    userManager.AddToRole(adminUser.Id, managerRole);
                }
            }

            string customerUserName = "londo";
            if (userManager.FindByName(customerUserName) == null)
            {
                var customerUser = new Account()
                {
                    UserName = customerUserName,
                    Email = "dochet1989@gmail.com",
                    EmailConfirmed = true,
                    PhoneNumber = "0987654321",
                    PhoneNumberConfirmed = true,
                    Profile = new UserProfile()
                    {
                        FullName = "Nguyễn Văn A",
                        Address = "120 Yên Lãng, Hà Nội",
                        Birthday = new DateTime(1990, 01, 15),
                        Gender = true,
                        CreatedDate = DateTime.Now
                    }
                };

                var result = userManager.Create(customerUser, "123456");

                if (result.Succeeded)
                {
                    userManager.AddToRole(customerUser.Id, customerRole);
                }
            }
        }
    }
}