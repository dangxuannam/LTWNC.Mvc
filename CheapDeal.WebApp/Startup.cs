using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CheapDeal.WebApp.Startup))]
namespace CheapDeal.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
