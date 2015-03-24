using System.Web.Mvc;
using System.Web.Routing;

namespace VotingApplication.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "DefaultAction",
                url: "{controller}/{action}/{id}/{token}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional, token = UrlParameter.Optional }
            );
        }
    }
}
