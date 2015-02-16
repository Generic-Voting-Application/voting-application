using System.Web;
using System.Web.Mvc;
using VotingApplication.Web.Api.Filters;

namespace VotingApplication.Web.Api
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LoggingHandleErrorAttribute());
        }
    }
}
