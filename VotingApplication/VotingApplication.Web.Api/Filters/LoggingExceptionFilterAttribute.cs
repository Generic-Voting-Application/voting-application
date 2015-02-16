using System.Web.Http.Filters;
using VotingApplication.Web.Api.Logging;

namespace VotingApplication.Web.Api.Filters
{
    public class LoggingExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            ILogger logger = LoggerFactory.GetLogger();

            logger.Log(actionExecutedContext.Exception.Message, actionExecutedContext.Exception);

            base.OnException(actionExecutedContext);
        }
    }
}