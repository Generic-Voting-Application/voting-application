using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VotingApplication.Web.Startup))]
namespace VotingApplication.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
