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
using System.Linq.Expressions;

namespace VotingApplication.Web.Api.Controllers
{
    public class WebApiController : ApiController
    {
        protected readonly IContextFactory _contextFactory;
        private readonly IMetricEventHandler _metricHandler;

        public WebApiController()
        {
            _contextFactory = new ContextFactory();
            _metricHandler = new MetricEventHandler(_contextFactory);
        }

        public WebApiController(IContextFactory contextFactory, IMetricEventHandler metricHandler)
        {
            _contextFactory = contextFactory;
            _metricHandler = metricHandler;
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

            if (_metricHandler != null)
            {
                _metricHandler.ErrorEvent(exception, CurrentPollId());
            }

            throw exception;
        }

        public void ThrowError(HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            HttpResponseException exception = new HttpResponseException(Request.CreateErrorResponse(statusCode, modelState));

            ILogger logger = LoggerFactory.GetLogger();
            logger.Log(exception);

            if (_metricHandler != null)
            {
                _metricHandler.ErrorEvent(exception, CurrentPollId());
            }

            throw exception;
        }

        public Poll PollByPollId(Guid pollId)
        {
            return PollByPredicate(p => p.UUID == pollId, string.Format("Poll {0} not found", pollId));
        }

        public Poll PollByManageId(Guid manageId)
        {
            return PollByPredicate(p => p.ManageId == manageId, string.Format("Poll for manage id {0} not found", manageId));
        }

        private Poll PollByPredicate(Expression<Func<Poll, bool>> predicate, string notFoundMessage)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls
                    .Where(predicate)
                    .Include(p => p.Options)
                    .Include(p => p.Ballots)
                    .Include(p => p.Ballots.Select(b => b.Votes))
                    .Include(p => p.Ballots.Select(b => b.Votes.Select(v => v.Option)))
                    .SingleOrDefault();

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, notFoundMessage);
                }

                return poll;
            }
        }

        private Guid CurrentPollId()
        {
            if (RequestContext.RouteData == null)
            {
                return Guid.Empty;
            }

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
                Guid manageId = Guid.Parse((string)routeValues["manageId"]);
                // Avoid using PollByManageId as this can infinitely recurse through
                // CurrentPollId => PollByManageId => ThrowError => CurrentPollId => ...
                Poll matchingPoll = context.Polls.Where(p => p.ManageId == manageId).SingleOrDefault();
                return (matchingPoll != null) ? matchingPoll.UUID : Guid.Empty;
            }
        }
    }
}