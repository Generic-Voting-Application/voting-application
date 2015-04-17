using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Linq;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Logging;
using VotingApplication.Web.Api.Metrics;

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

            MetricEventHandler.ErrorEvent(exception, CurrentPollId());

            throw exception;
        }

        public void ThrowError(HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            HttpResponseException exception = new HttpResponseException(Request.CreateErrorResponse(statusCode, modelState));

            ILogger logger = LoggerFactory.GetLogger();
            logger.Log(exception);
            MetricEventHandler.ErrorEvent(exception, CurrentPollId());

            throw exception;
        }

        private Guid CurrentPollId()
        {
            IDictionary<string, object> routeValues = RequestContext.RouteData.Values;

            if (routeValues["pollId"] != null)
            {
                return Guid.Parse((string)routeValues["pollId"]);
            }

            if (routeValues["manageId"] == null)
            {
                return Guid.Empty;
            }

            // Find corresponding pollId for manageId
            using (var context = _contextFactory.CreateContext())
            {
                Guid manageGuid = Guid.Parse((string)routeValues["manageId"]);
                Poll matchingPoll = context.Polls.SingleOrDefault(p => p.ManageId == manageGuid);

                if (matchingPoll == null)
                {
                    return Guid.Empty;
                }

                return matchingPoll.UUID;
            }
        }
    }
}