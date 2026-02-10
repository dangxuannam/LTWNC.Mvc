using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {   
            var rolesList = new List<string> { "Admin", "User", "Manager", "Staff" };

            ViewBag.Roles = new SelectList(rolesList);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}