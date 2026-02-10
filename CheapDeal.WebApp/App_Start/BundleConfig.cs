using System.Web;
using System.Web.Optimization;

namespace CheapDeal.WebApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/admmainjs").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstraps.js",
                        "~/Scripts/respond.js",
                        "~/Scripts/plugins/metisMenu/jquery.metisMenu.js",
                        "~/Scripts/plugins/slimscroll/jquery.slimscroll.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery.validate.bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/admcustomjs").Include(
                        "~/Scripts/cheapdeal.js",
                        "~/Scripts/plugins/pace/pace.min.js"));

            bundles.Add(new StyleBundle("~/Content/admbootstrap").Include(
                      "~/Content/bootstrap-3.3.0.min.css",
                      "~/Content/font-awesome.css"));

            bundles.Add(new StyleBundle("~/Content/admin-styles").Include(
                      "~/Content/animate.css",
                      "~/Content/Admsite.css",
                        "~/Content/font-awesome.css"));
            bundles.Add(new StyleBundle("~/Content/inspinia").Include(
                "~/Content/bootstrap.min.css",
                 "~/Content/font-awesome.css",
                "~/Content/animate.css",
                "~/Content/style.css"));  // Style chính Inspinia

            // JS bundles
            bundles.Add(new ScriptBundle("~/bundles/inspinia-js").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/plugins/metisMenu/jquery.metisMenu.js",
                "~/Scripts/plugins/slimscroll/jquery.slimscroll.min.js",
                "~/Scripts/plugins/pace/pace.min.js",
                "~/Scripts/app/inspinia.js"));
        }
    }
}
