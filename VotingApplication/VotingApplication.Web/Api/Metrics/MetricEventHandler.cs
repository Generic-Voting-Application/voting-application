using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
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

            errorEvent.StatusCode = (int)exception.Response.StatusCode;

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
            pageChangeEvent.StatusCode = statusCode;
            pageChangeEvent.Detail = route;
            StoreEvent(pageChangeEvent);
        }

        public void ResultsUpdateEvent(HttpStatusCode status, Guid pollId)
        {
            Event updateResultsEvent = new Event(EventType.UpdateResults, pollId);
            updateResultsEvent.StatusCode = (int)status;
            StoreEvent(updateResultsEvent);
        }

        #endregion

        public void PollCreatedEvent(Poll poll)
        {
            Event pollCreatedEvent = new Event(EventType.CreatePoll, poll.UUID);
            pollCreatedEvent.Value = poll.Name;
            pollCreatedEvent.Detail = new JavaScriptSerializer().Serialize(poll);
            StoreEvent(pollCreatedEvent);
        }

        public void PollClonedEvent(Poll poll)
        {
            Event pollClonedEvent = new Event(EventType.ClonePoll, poll.UUID);
            pollClonedEvent.Value = poll.Name;
            pollClonedEvent.Detail = new JavaScriptSerializer().Serialize(poll);
            StoreEvent(pollClonedEvent);
        }

        #region Poll Configuration

        public void ExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId)
        {
            Event setExpiryEvent = new Event(EventType.SetExpiry, pollId);
            setExpiryEvent.Value = (expiry != null) ? expiry.ToString() : "Never";
            StoreEvent(setExpiryEvent);
        }

        public void PollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId)
        {
            Event setPollType = new Event(EventType.SetPollType, pollId);
            setPollType.Value = pollType.ToString();

            if (pollType == PollType.Points)
            {
                setPollType.Detail = "{ MaxPerPoll: " + maxPerPoll + ", MaxPerVote: " + maxPerVote + " }";
            }

            StoreEvent(setPollType);
        }

        public void QuestionChangedEvent(string question, Guid pollId)
        {
            Event setQuestion = new Event(EventType.SetQuestion, pollId);
            setQuestion.Value = question;
            StoreEvent(setQuestion);
        }

        #region Misc Configuration

        public void InviteOnlyChangedEvent(bool inviteOnly, Guid pollId)
        {
            Event setInviteOnly = new Event(EventType.SetInviteOnly, pollId);
            setInviteOnly.Value = (inviteOnly) ? "True" : "False";
            setInviteOnly.Detail = (inviteOnly) ? "Invite-Only" : "Open Poll";
            StoreEvent(setInviteOnly);
        }

        public void NamedVotingChangedEvent(bool namedVoting, Guid pollId)
        {
            Event setNamedVoting = new Event(EventType.SetNamedVoting, pollId);
            setNamedVoting.Value = (namedVoting) ? "True" : "False";
            setNamedVoting.Detail = (namedVoting) ? "Named Voters" : "Anonymous Voters";
            StoreEvent(setNamedVoting);
        }

        public void OptionAddingChangedEvent(bool optionAdding, Guid pollId)
        {
            Event setAllowOptionAdding = new Event(EventType.SetOptionAdding, pollId);
            setAllowOptionAdding.Value = (optionAdding) ? "True" : "False";
            setAllowOptionAdding.Detail = (optionAdding) ? "Voter Option Adding" : "No Voter Option Adding";
            StoreEvent(setAllowOptionAdding);
        }

        public void HiddenResultsChangedEvent(bool hiddenResults, Guid pollId)
        {
            Event setHiddenResults = new Event(EventType.SetHiddenResults, pollId);
            setHiddenResults.Value = (hiddenResults) ? "True" : "False";
            setHiddenResults.Detail = (hiddenResults) ? "Results hidden before voting" : "Results visible before voting";
            StoreEvent(setHiddenResults);
        }

        #endregion

        #region Options

        public void OptionAddedEvent(Option option, Guid pollId)
        {
            Event optionAddedEvent = new Event(EventType.AddOption, pollId);
            optionAddedEvent.Value = option.Name;
            optionAddedEvent.Detail = string.Format("#{0} '{1}'", option.PollOptionNumber, option.Description);
            StoreEvent(optionAddedEvent);
        }

        public void OptionUpdatedEvent(Option option, Guid pollId)
        {
            Event optionUpdatedEvent = new Event(EventType.UpdateOption, pollId);
            optionUpdatedEvent.Value = option.Name;
            optionUpdatedEvent.Detail = string.Format("#{0} '{1}'", option.PollOptionNumber, option.Description);
            StoreEvent(optionUpdatedEvent);
        }

        public void OptionDeletedEvent(Option option, Guid pollId)
        {
            Event optionDeletedEvent = new Event(EventType.DeleteOption, pollId);
            optionDeletedEvent.Value = option.Name;
            optionDeletedEvent.Detail = string.Format("#{0} '{1}'", option.PollOptionNumber, option.Description);
            StoreEvent(optionDeletedEvent);
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

        public void LoginEvent(string username)
        {
            Event loginEvent = new Event(EventType.Login, Guid.Empty);
            loginEvent.Value = username;
            StoreEvent(loginEvent);
        }

        public void RegisterEvent(string username)
        {
            Event registerEvent = new Event(EventType.Register, Guid.Empty);
            registerEvent.Value = username;
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