using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Context;
using VotingApplication.Web.Api.Logging;

namespace VotingApplication.Web.Api.Controllers
{
    public class WebApiController : ApiController
    {
        protected readonly IContextFactory _contextFactory;

        public WebApiController()
        {
            this._contextFactory = new ContextFactory();
        }

        public WebApiController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        public void ThrowError(HttpStatusCode statusCode)
        {
            ThrowError(statusCode, String.Empty);
        }

        public void ThrowError(HttpStatusCode statusCode, string message)
        {
            HttpResponseException exception = new HttpResponseException(new HttpResponseMessage(statusCode)
            {
                ReasonPhrase = message
            });

            ILogger logger = LoggerFactory.GetLogger();

            logger.Log(message, exception);

            throw exception;
        }

        public void ThrowError(HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            HttpResponseException exception = new HttpResponseException(Request.CreateErrorResponse(statusCode, modelState));

            ILogger logger = LoggerFactory.GetLogger();
            logger.Log(exception);

            throw exception;
        }
    }
}