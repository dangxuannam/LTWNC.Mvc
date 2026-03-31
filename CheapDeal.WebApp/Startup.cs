using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.Dashboard;

[assembly: OwinStartup(typeof(CheapDeal.WebApp.Startup))]
namespace CheapDeal.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            ConfigureAuth(app);

            // ── 2. Hangfire ─────────────────────────────────────
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAllFilter() }
            });
            app.UseHangfireServer();
        }
    }

    public class AllowAllFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}