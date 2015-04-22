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
            Event errorEvent = new Event(EventType.Error, GetExistingPollId(pollId));

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

        #region Page Loads

        public void PageChangeEvent(string route, int statusCode, Guid pollId)
        {
            Event pageChangeEvent = new Event(EventType.GoToPage, GetExistingPollId(pollId));
            pageChangeEvent.Value = ((HttpStatusCode)statusCode).ToString();
            pageChangeEvent.Detail = route;
            StoreEvent(pageChangeEvent);
        }

        public void ResultsUpdateEvent(HttpStatusCode status, Guid pollId)
        {
            Event updateResultsEvent = new Event(EventType.UpdateResults, pollId);
            updateResultsEvent.Value = status.ToString();
            StoreEvent(updateResultsEvent);
        }

        #endregion

        #region Poll Configuration

        public void ExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId)
        {
            Event setExpiryEvent = new Event(EventType.SetExpiry, pollId);
            setExpiryEvent.Detail = (expiry != null) ? expiry.ToString() : "Never";
            StoreEvent(setExpiryEvent);
        }

        public void PollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId)
        {
            Event setPollType = new Event(EventType.SetPollType, pollId);
            setPollType.Detail = pollType.ToString();

            if (pollType == PollType.Points)
            {
                setPollType.Detail += " (" + maxPerPoll + "/" + maxPerVote + ")";
            }

            StoreEvent(setPollType);
        }

        public void QuestionChangedEvent(string question, Guid pollId)
        {
            Event setQuestion = new Event(EventType.SetQuestion, pollId);
            setQuestion.Detail = question;
            StoreEvent(setQuestion);
        }

        #region Misc Configuration

        public void InviteOnlyChangedEvent(bool inviteOnly, Guid pollId)
        {
            Event setInviteOnly = new Event(EventType.SetInviteOnly, pollId);
            setInviteOnly.Detail = (inviteOnly) ? "Invite-Only" : "Open Poll";
            StoreEvent(setInviteOnly);
        }

        public void NamedVotingChangedEvent(bool namedVoting, Guid pollId)
        {
            Event setNamedVoting = new Event(EventType.SetNamedVoting, pollId);
            setNamedVoting.Detail = (namedVoting) ? "Named Voters" : "Anonymous Voters";
            StoreEvent(setNamedVoting);
        }

        public void OptionAddingChangedEvent(bool optionAdding, Guid pollId)
        {
            Event setAllowOptionAdding = new Event(EventType.SetOptionAdding, pollId);
            setAllowOptionAdding.Detail = (optionAdding) ? "Voter Option Adding" : "No Voter Option Adding";
            StoreEvent(setAllowOptionAdding);
        }

        #endregion

        #region Options

        public void OptionAddedEvent(Option option, Guid pollId)
        {
            Event optionAddedEvent = new Event(EventType.AddOption, pollId);
            optionAddedEvent.Detail = string.Format("#{0} '{1}': '{2}'", option.PollOptionNumber, option.Name, option.Description);
            StoreEvent(optionAddedEvent);
        }

        public void OptionUpdatedEvent(Option option, Guid pollId)
        {
            Event optionUpdatedEvent = new Event(EventType.UpdateOption, pollId);
            optionUpdatedEvent.Detail = string.Format("#{0} '{1}': '{2}'", option.PollOptionNumber, option.Name, option.Description);
            StoreEvent(optionUpdatedEvent);
        }

        public void OptionDeletedEvent(Option option, Guid pollId)
        {
            Event optionAddedEvent = new Event(EventType.DeleteOption, pollId);
            optionAddedEvent.Detail = string.Format("#{0} '{1}': '{2}'", option.PollOptionNumber, option.Name, option.Description);
            StoreEvent(optionAddedEvent);
        }

        #endregion

        #region Votes

        public void VoteAddedEvent(Vote vote, Guid pollId)
        {
            Event voteAddedEvent = new Event(EventType.AddVote, pollId);
            voteAddedEvent.Value = vote.Option.PollOptionNumber.ToString();
            voteAddedEvent.Detail = vote.Option.Name + " (" + vote.VoteValue + ")";
            StoreEvent(voteAddedEvent);
        }

        public void VoteDeletedEvent(Vote vote, Guid pollId)
        {
            Event voteAddedEvent = new Event(EventType.DeleteVote, pollId);
            voteAddedEvent.Value = vote.Option.PollOptionNumber.ToString();
            voteAddedEvent.Detail = vote.Option.Name + " (" + vote.VoteValue + ")";
            StoreEvent(voteAddedEvent);
        }

        #endregion

        #region Ballot

        public void BallotAddedEvent(Ballot ballot, Guid pollId)
        {
            Event ballotAddedEvent = new Event(EventType.AddBallot, pollId);
            ballotAddedEvent.Value = ballot.TokenGuid.ToString();
            ballotAddedEvent.Detail = ballot.Email;
            StoreEvent(ballotAddedEvent);
        }

        public void BallotDeletedEvent(Ballot ballot, Guid pollId)
        {
            Event ballotDeletedEvent = new Event(EventType.DeleteBallot, pollId);
            ballotDeletedEvent.Value = ballot.TokenGuid.ToString();
            ballotDeletedEvent.Detail = ballot.Email;
            StoreEvent(ballotDeletedEvent);
        }

        #endregion

        #endregion

        #region Accounts

        public void LoginEvent()
        {
            Event loginEvent = new Event(EventType.Login, Guid.Empty);
            loginEvent.Value = HttpStatusCode.OK.ToString();
            StoreEvent(loginEvent);
        }

        public void RegisterEvent()
        {
            Event registerEvent = new Event(EventType.Register, Guid.Empty);
            registerEvent.Value = HttpStatusCode.OK.ToString();
            StoreEvent(registerEvent);
        }

        #endregion

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