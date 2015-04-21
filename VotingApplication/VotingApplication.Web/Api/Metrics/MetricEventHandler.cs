using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public class MetricEventHandler : IMetricEventHandler
    {
        private readonly IContextFactory _contextFactory;
        
        public MetricEventHandler (IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region Error Events

        public void ErrorEvent(HttpResponseException exception, Guid pollId)
        {
            Event errorEvent = new Event("ERROR", GetExistingPollId(pollId));

            errorEvent.Value = exception.Response.StatusCode.ToString();

            if (exception.Response.RequestMessage != null)
            {
                errorEvent.Detail = ErrorDetailFromRequestMessage(exception);
            }
            else
            {
                errorEvent.Detail = exception.Response.ReasonPhrase;
            }

            StoreEvent(errorEvent);
        }

        private string ErrorDetailFromRequestMessage(HttpResponseException exception)
        {
            string apiRoute = exception.Response.RequestMessage.Method + " " + exception.Response.RequestMessage.RequestUri;
            string requestPayload = GetPayload(exception);
            return apiRoute + " " + requestPayload;
        }

        #endregion

        public void PageChangeEvent(string route, int statusCode, Guid pollId)
        {
            Event pageChangeEvent = new Event("GoTo" + route, GetExistingPollId(pollId));
            pageChangeEvent.Value = ((HttpStatusCode)statusCode).ToString();
            StoreEvent(pageChangeEvent);
        }

        public void ResultsUpdateEvent(HttpStatusCode status, Guid pollId)
        {
            Event updateResultsEvent = new Event("UpdateResults", pollId);
            updateResultsEvent.Value = status.ToString();
            StoreEvent(updateResultsEvent);
        }

        #region Poll Configuration

        public void ExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId)
        {
            Event setExpiryEvent = new Event("SetExpiry", pollId);
            setExpiryEvent.Value = HttpStatusCode.OK.ToString();
            setExpiryEvent.Detail = (expiry != null) ? expiry.ToString() : "Never";
            StoreEvent(setExpiryEvent);
        }

        public void PollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId)
        {
            Event setPollType = new Event("SetPollType", pollId);
            setPollType.Value = HttpStatusCode.OK.ToString();
            setPollType.Detail = pollType.ToString();

            if (pollType == PollType.Points)
            {
                setPollType.Detail += " (" + maxPerPoll + "/" + maxPerVote + ")";
            }

            StoreEvent(setPollType);
        }

        public void InviteOnlyChangedEvent(bool inviteOnly, Guid pollId)
        {
            Event setInviteOnly = new Event("SetInviteOnly", pollId);
            setInviteOnly.Value = HttpStatusCode.OK.ToString();
            setInviteOnly.Detail = (inviteOnly) ? "Invite-Only" : "Open Poll";
            StoreEvent(setInviteOnly);
        }

        public void NamedVotingChangedEvent(bool namedVoting, Guid pollId)
        {
            Event setNamedVoting = new Event("SetNamedVoting", pollId);
            setNamedVoting.Value = HttpStatusCode.OK.ToString();
            setNamedVoting.Detail = (namedVoting) ? "Named Voters" : "Anonymous Voters";
            StoreEvent(setNamedVoting);
        }

        public void OptionAddingChangedEvent(bool optionAdding, Guid pollId)
        {
            Event setAllowOptionAdding = new Event("SetInviteOnly", pollId);
            setAllowOptionAdding.Value = HttpStatusCode.OK.ToString();
            setAllowOptionAdding.Detail = (optionAdding) ? "Voter Option Adding" : "No Voter Option Adding";
            StoreEvent(setAllowOptionAdding);
        }

        #endregion

        public void LoginEvent()
        {
            Event loginEvent = new Event("Login", Guid.Empty);
            loginEvent.Value = HttpStatusCode.OK.ToString();
            StoreEvent(loginEvent);
        }

        public void RegisterEvent()
        {
            Event registerEvent = new Event("Register", Guid.Empty);
            registerEvent.Value = HttpStatusCode.OK.ToString();
            StoreEvent(registerEvent);
        }

        #region Utilities

        private string GetPayload(HttpResponseException exception)
        {
            var requestContext = exception.Response.RequestMessage.Properties["MS_RequestContext"];
            HttpRequestWrapper webRequest = requestContext.GetType().GetProperty("WebRequest").GetValue(requestContext) as HttpRequestWrapper;

            if (webRequest == null)
            {
                return "";
            }

            byte[] buffer = new byte[webRequest.ContentLength];
            webRequest.InputStream.Read(buffer, 0, webRequest.ContentLength);
            return System.Text.Encoding.Default.GetString(buffer);
        }

        // Make sure we are using the PollId, not the corresponding ManageId, if available
        private Guid GetExistingPollId(Guid guid)
        {
            // Find corresponding pollId for manageId
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(p => p.UUID == guid || p.ManageId == guid).SingleOrDefault();
                return (matchingPoll != null) ? matchingPoll.UUID : Guid.Empty;
            }
        }

        private void StoreEvent(Event eventToStore)
        {
            using (var context = _contextFactory.CreateContext())
            {
                context.Events.Add(eventToStore);
                context.SaveChanges();
            }
        }

        #endregion
    }
}