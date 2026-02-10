using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CheapDeal.WebApp.Areas.Adm.Controllers
{
    [Authorize(Roles ="Admin,Manager,Salesman")]
    
    public class AdminController : Controller
    {
        protected virtual ActionResult Redirect (object id )
        {
            var saveAction = Request.Form ["save-action"];
            switch (saveAction)
            {
                case "save-new":
                    return RedirectToAction("Create");
                case "save-edit":
                    return RedirectToAction("Edit", new { id });

                default:
                    return RedirectToAction("Index");
            }
        }

        protected virtual bool OnUpdateToggle (
            string propName, bool value,object[] keys)
        {
            return true;
        }

        [HttpPost]
        public JsonResult UpdateToggle(string args)
        {
            bool success = false;
            string html = string.Empty;

            try
            {
                var data = args.Split('_');
                var propName = data[0];
                var value = !bool.Parse(data[1]);
                var keys = data.Skip(2).ToArray();

                if (OnUpdateToggle(propName, value, keys))
                {
                    success = true;

                    html = String.Format("{0},{1},{2}",
                        propName,
                        value.ToString().ToLower(),
                        string.Join("_", keys));
                }
            }
            catch (Exception ex) {
                success = false;
                html = ex.Message;
            }
            return Json(new
            {
                Result  = success,
                Message = html });
        }
      
        // GET: Adm/Admin
        
    }
}