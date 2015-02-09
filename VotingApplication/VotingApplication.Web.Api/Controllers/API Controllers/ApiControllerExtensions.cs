using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using VotingApplication.Web.Api.Logging;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public static class ApiControllerExtensions
    {
        public static void ThrowError(this ApiController controller, HttpStatusCode statusCode, string message = "")
        {
            HttpResponseException exception = new HttpResponseException(new HttpResponseMessage(statusCode)
            {
                ReasonPhrase = message
            });

            ILogger logger = new LoggerFactory().GetLogger();
            logger.Log(message, exception);

            throw exception;
        }

        public static void ThrowError(this ApiController controller, HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            HttpResponseException exception = new HttpResponseException(controller.Request.CreateErrorResponse(statusCode, modelState));

            ILogger logger = new LoggerFactory().GetLogger();
            logger.Log(exception);

            throw exception;
        }
    }
}