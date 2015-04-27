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
    public class EmptyMetricHandler : IMetricHandler
    {
        public void HandlePageChangeEvent(string route, int statusCode, Guid pollId) { }
        public void HandleErrorEvent(HttpResponseException exception, Guid pollId) { }

        public void HandlePollCreatedEvent(Poll poll) { }
        public void HandlePollClonedEvent(Poll poll) { }
        public void HandleResultsUpdateEvent(HttpStatusCode status, Guid pollId) { }

        public void HandleExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId) { }
        public void HandlePollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId) { }
        public void HandleInviteOnlyChangedEvent(bool inviteOnly, Guid pollId) { }
        public void HandleNamedVotingChangedEvent(bool namedVoting, Guid pollId) { }
        public void HandleOptionAddingChangedEvent(bool optionAdding, Guid pollId) { }
        public void HandleHiddenResultsChangedEvent(bool hiddenResults, Guid pollId) { }
        public void HandleQuestionChangedEvent(string question, Guid pollId) { }

        public void HandleOptionAddedEvent(Option option, Guid pollId) { }
        public void HandleOptionUpdatedEvent(Option option, Guid pollId) { }
        public void HandleOptionDeletedEvent(Option option, Guid pollId) { }

        public void HandleVoteAddedEvent(Vote vote, Guid pollId) { }
        public void HandleVoteDeletedEvent(Vote vote, Guid pollId) { }

        public void HandleBallotAddedEvent(Ballot ballot, Guid pollId) { }
        public void HandleBallotDeletedEvent(Ballot ballot, Guid pollId) { }

        public void HandleLoginEvent(string username) { }
        public void HandleRegisterEvent(string username) { }
    }
}