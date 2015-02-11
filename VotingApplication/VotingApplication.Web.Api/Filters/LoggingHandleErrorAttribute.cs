using System.Web.Mvc;
using VotingApplication.Web.Api.Logging;

namespace VotingApplication.Web.Api.Filters
{
    public class LoggingHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            ILogger logger = LoggerFactory.GetLogger();

            logger.Log(filterContext.Exception.Message, filterContext.Exception);

            base.OnException(filterContext);
        }
    }
}