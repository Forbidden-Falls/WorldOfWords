using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WorldOfWords.Web.Startup))]
namespace WorldOfWords.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
