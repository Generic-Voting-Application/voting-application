using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;

namespace VotingApplication.Web.Api.Controllers
{
    public class WebApiController : ApiController
    {
        protected readonly IContextFactory _contextFactory;
        protected readonly IMetricHandler _metricHandler;

        public WebApiController()
        {
            _contextFactory = new ContextFactory();
            _metricHandler = new MetricHandler(_contextFactory);
        }

        public WebApiController(IContextFactory contextFactory, IMetricHandler metricHandler)
        {
            _contextFactory = contextFactory;
            _metricHandler = metricHandler ?? new EmptyMetricHandler();
        }

        public void ThrowError(HttpStatusCode statusCode)
        {
            ThrowError(statusCode, String.Empty);
        }

        public void ThrowError(HttpStatusCode statusCode, string message)
        {
            var httpResponseMessage = new HttpResponseMessage(statusCode);
            if (!String.IsNullOrWhiteSpace(message))
            {
                httpResponseMessage.ReasonPhrase = message;
            }

            HttpResponseException exception = new HttpResponseException(httpResponseMessage);

            if (_metricHandler != null)
            {
                _metricHandler.HandleErrorEvent(exception, CurrentPollId());
            }

            throw exception;
        }

        public void ThrowError(HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            HttpResponseException exception = new HttpResponseException(Request.CreateErrorResponse(statusCode, modelState));

            _metricHandler.HandleErrorEvent(exception, CurrentPollId());

            throw exception;
        }

        protected internal Poll PollByPollId(Guid pollId, IVotingContext context)
        {
            Expression<Func<Poll, bool>> predicate = (p => p.UUID == pollId);
            string errorMessage = string.Format("Poll {0} not found", pollId);

            return PollByPredicate(predicate, errorMessage, context);
        }

        protected internal Poll PollByManageId(Guid manageId, IVotingContext context)
        {
            Expression<Func<Poll, bool>> predicate = (p => p.ManageId == manageId);
            string errorMessage = string.Format("Poll for manage id {0} not found", manageId);

            Poll poll = PollByPredicate(predicate, errorMessage, context);

            if (String.IsNullOrEmpty(poll.CreatorIdentity) && !String.IsNullOrEmpty(User.Identity.GetUserId()))
            {
                poll.CreatorIdentity = User.Identity.GetUserId();
                poll.Creator = User.Identity.GetUserName();
                context.SaveChanges();
            }

            return poll;
        }

        private Poll PollByPredicate(Expression<Func<Poll, bool>> predicate, string notFoundMessage, IVotingContext context)
        {
            Poll poll = context.Polls
                .Include(p => p.Choices)
                .Include(p => p.Ballots)
                .Include(p => p.Ballots.Select(b => b.Votes))
                .Include(p => p.Ballots.Select(b => b.Votes.Select(v => v.Choice)))
                .SingleOrDefault(predicate);

            if (poll == null)
            {
                ThrowError(HttpStatusCode.NotFound, notFoundMessage);
            }

            return poll;
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

            if (routeValues["manageId"] != null)
            {
                return Guid.Parse((string)routeValues["manageId"]);
            }

            return Guid.Empty;
        }

        protected Guid? GetTokenGuidFromHeaders()
        {
            IEnumerable<string> tokenHeaders;
            bool success = Request.Headers.TryGetValues("X-TokenGuid", out tokenHeaders);

            if (success)
            {
                if (tokenHeaders.Count() > 1)
                {
                    ThrowError(HttpStatusCode.BadRequest, "Multiple X-TokenGuid headers");
                }

                return new Guid(tokenHeaders.Single());
            }

            return null;
        }
    }
}