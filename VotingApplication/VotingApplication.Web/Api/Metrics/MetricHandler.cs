using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public class MetricHandler : IMetricHandler
    {
        private readonly IContextFactory _contextFactory;

        public MetricHandler(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region Error Events

        public void HandleErrorEvent(HttpResponseException exception, Guid pollId)
        {
            HttpResponseMessage response = exception.Response;
            Metric errorEvent = new Metric(MetricType.Error, GetExistingPollId(pollId));

            errorEvent.StatusCode = (int)response.StatusCode;

            if (response.RequestMessage != null)
            {
                errorEvent.Detail = ErrorDetailFromRequestMessage(response.RequestMessage);
            }
            else
            {
                errorEvent.Detail = response.ReasonPhrase;
            }

            StoreEvent(errorEvent);
        }

        private static string ErrorDetailFromRequestMessage(HttpRequestMessage request)
        {
            string apiRoute = request.Method + " " + request.RequestUri;
            string requestPayload = GetPayload(request);
            return apiRoute + " " + requestPayload;
        }

        #endregion

        #region Page Loads

        public void HandlePageChangeEvent(string route, int statusCode, Guid pollId)
        {
            Metric pageChangeEvent = new Metric(MetricType.GoToPage, GetExistingPollId(pollId));
            pageChangeEvent.StatusCode = statusCode;
            pageChangeEvent.Detail = route;
            StoreEvent(pageChangeEvent);
        }

        public void HandleResultsUpdateEvent(HttpStatusCode status, Guid pollId)
        {
            Metric updateResultsEvent = new Metric(MetricType.UpdateResults, pollId);
            updateResultsEvent.StatusCode = (int)status;
            StoreEvent(updateResultsEvent);
        }

        #endregion

        public void HandlePollCreatedEvent(Poll poll)
        {
            Metric pollCreatedEvent = new Metric(MetricType.CreatePoll, poll.UUID);
            pollCreatedEvent.Value = poll.Name;
            pollCreatedEvent.Detail = new JavaScriptSerializer().Serialize(poll);
            StoreEvent(pollCreatedEvent);
        }

        public void HandlePollClonedEvent(Poll poll)
        {
            Metric pollClonedEvent = new Metric(MetricType.ClonePoll, poll.UUID);
            pollClonedEvent.Value = poll.Name;
            pollClonedEvent.Detail = new JavaScriptSerializer().Serialize(poll);
            StoreEvent(pollClonedEvent);
        }

        #region Poll Configuration

        public void HandleExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId)
        {
            Metric setExpiryEvent = new Metric(MetricType.SetExpiry, pollId);
            if (expiry == null)
            {
                setExpiryEvent.Value = "Never";
            }
            else
            {
                setExpiryEvent.Value = string.Format("{0:dd/MM/yyyy HH:mm:ss zzz}", expiry);
            }
            StoreEvent(setExpiryEvent);
        }

        public void HandlePollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId)
        {
            Metric setPollType = new Metric(MetricType.SetPollType, pollId);
            setPollType.Value = pollType.ToString();

            if (pollType == PollType.Points)
            {
                setPollType.Detail = "{ MaxPerPoll: " + maxPerPoll + ", MaxPerVote: " + maxPerVote + " }";
            }

            StoreEvent(setPollType);
        }

        public void HandleQuestionChangedEvent(string question, Guid pollId)
        {
            Metric setQuestion = new Metric(MetricType.SetQuestion, pollId);
            setQuestion.Value = question;
            StoreEvent(setQuestion);
        }

        #region Misc Configuration

        public void HandleInviteOnlyChangedEvent(bool inviteOnly, Guid pollId)
        {
            Metric setInviteOnly = new Metric(MetricType.SetInviteOnly, pollId);
            setInviteOnly.Value = (inviteOnly) ? "True" : "False";
            setInviteOnly.Detail = (inviteOnly) ? "Invite-Only" : "Open Poll";
            StoreEvent(setInviteOnly);
        }

        public void HandleNamedVotingChangedEvent(bool namedVoting, Guid pollId)
        {
            Metric setNamedVoting = new Metric(MetricType.SetNamedVoting, pollId);
            setNamedVoting.Value = (namedVoting) ? "True" : "False";
            setNamedVoting.Detail = (namedVoting) ? "Named Voters" : "Anonymous Voters";
            StoreEvent(setNamedVoting);
        }

        public void HandleChoiceAddingChangedEvent(bool optionAdding, Guid pollId)
        {
            Metric setAllowOptionAdding = new Metric(MetricType.SetOptionAdding, pollId);
            setAllowOptionAdding.Value = (optionAdding) ? "True" : "False";
            setAllowOptionAdding.Detail = (optionAdding) ? "Voter Option Adding" : "No Voter Option Adding";
            StoreEvent(setAllowOptionAdding);
        }

        public void HandleElectionModeChangedEvent(bool electionMode, Guid pollId)
        {
            Metric setElectionMode = new Metric(MetricType.SetElectionMode, pollId);
            setElectionMode.Value = (electionMode) ? "True" : "False";
            setElectionMode.Detail = (electionMode) ? "Results hidden before voting" : "Results visible before voting";
            StoreEvent(setElectionMode);
        }

        #endregion

        #region Options

        public void HandleChoiceAddedEvent(Choice option, Guid pollId)
        {
            Metric optionAddedEvent = new Metric(MetricType.AddOption, pollId);
            optionAddedEvent.Value = option.Name;
            optionAddedEvent.Detail = string.Format("#{0} '{1}'", option.PollChoiceNumber, option.Description);
            StoreEvent(optionAddedEvent);
        }

        public void HandleChoiceUpdatedEvent(Choice option, Guid pollId)
        {
            Metric optionUpdatedEvent = new Metric(MetricType.UpdateOption, pollId);
            optionUpdatedEvent.Value = option.Name;
            optionUpdatedEvent.Detail = string.Format("#{0} '{1}'", option.PollChoiceNumber, option.Description);
            StoreEvent(optionUpdatedEvent);
        }

        public void HandleChoiceDeletedEvent(Choice option, Guid pollId)
        {
            Metric optionDeletedEvent = new Metric(MetricType.DeleteOption, pollId);
            optionDeletedEvent.Value = option.Name;
            optionDeletedEvent.Detail = string.Format("#{0} '{1}'", option.PollChoiceNumber, option.Description);
            StoreEvent(optionDeletedEvent);
        }

        #endregion

        #region Votes

        public void HandleVoteAddedEvent(Vote vote, Guid pollId)
        {
            Metric voteAddedEvent = new Metric(MetricType.AddVote, pollId);
            voteAddedEvent.Value = vote.Choice.PollChoiceNumber.ToString();
            voteAddedEvent.Detail = vote.Choice.Name + " (" + vote.VoteValue + ")";
            StoreEvent(voteAddedEvent);
        }

        public void HandleVoteDeletedEvent(Vote vote, Guid pollId)
        {
            Metric voteAddedEvent = new Metric(MetricType.DeleteVote, pollId);
            voteAddedEvent.Value = vote.Choice.PollChoiceNumber.ToString();
            voteAddedEvent.Detail = vote.Choice.Name + " (" + vote.VoteValue + ")";
            StoreEvent(voteAddedEvent);
        }

        #endregion

        #region Ballot

        public void HandleBallotAddedEvent(Ballot ballot, Guid pollId)
        {
            Metric ballotAddedEvent = new Metric(MetricType.AddBallot, pollId);
            ballotAddedEvent.Value = ballot.TokenGuid.ToString();
            ballotAddedEvent.Detail = ballot.Email;
            StoreEvent(ballotAddedEvent);
        }

        public void HandleBallotDeletedEvent(Ballot ballot, Guid pollId)
        {
            Metric ballotDeletedEvent = new Metric(MetricType.DeleteBallot, pollId);
            ballotDeletedEvent.Value = ballot.TokenGuid.ToString();
            ballotDeletedEvent.Detail = ballot.Email;
            StoreEvent(ballotDeletedEvent);
        }

        #endregion

        #endregion

        #region Accounts

        public void HandleLoginEvent(string username)
        {
            Metric loginEvent = new Metric(MetricType.Login, Guid.Empty);
            loginEvent.Value = username;
            StoreEvent(loginEvent);
        }

        public void HandleRegisterEvent(string username)
        {
            Metric registerEvent = new Metric(MetricType.Register, Guid.Empty);
            registerEvent.Value = username;
            StoreEvent(registerEvent);
        }

        #endregion

        #region Utilities

        private static string GetPayload(HttpRequestMessage request)
        {
            var requestContext = request.Properties["MS_RequestContext"];
            HttpRequestWrapper webRequest = requestContext.GetType().GetProperty("WebRequest").GetValue(requestContext) as HttpRequestWrapper;

            if (webRequest == null)
            {
                return String.Empty;
            }

            byte[] buffer = new byte[webRequest.ContentLength];
            webRequest.InputStream.Read(buffer, 0, webRequest.ContentLength);
            return System.Text.Encoding.Default.GetString(buffer);
        }

        // Make sure we are using the PollId, not the corresponding ManageId, if available
        private Guid GetExistingPollId(Guid guid)
        {
            if(guid == Guid.Empty)
            {
                return Guid.Empty;
            }

            // Find corresponding pollId for manageId
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.SingleOrDefault(p => p.UUID == guid || p.ManageId == guid);

                if (matchingPoll == null)
                {
                    return Guid.Empty;
                }

                return matchingPoll.UUID;
            }
        }

        private void StoreEvent(Metric eventToStore)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                context.Metrics.Add(eventToStore);
                context.SaveChanges();
            }
        }

        #endregion
    }
}