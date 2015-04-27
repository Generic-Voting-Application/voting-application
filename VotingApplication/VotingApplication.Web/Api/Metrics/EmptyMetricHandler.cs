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
        public void PageChangeEvent(string route, int statusCode, Guid pollId) { }
        public void ErrorEvent(HttpResponseException exception, Guid pollId) { }

        public void PollCreatedEvent(Poll poll) { }
        public void PollClonedEvent(Poll poll) { }
        public void ResultsUpdateEvent(HttpStatusCode status, Guid pollId) { }

        public void ExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId) { }
        public void PollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId) { }
        public void InviteOnlyChangedEvent(bool inviteOnly, Guid pollId) { }
        public void NamedVotingChangedEvent(bool namedVoting, Guid pollId) { }
        public void OptionAddingChangedEvent(bool optionAdding, Guid pollId) { }
        public void HiddenResultsChangedEvent(bool hiddenResults, Guid pollId) { }
        public void QuestionChangedEvent(string question, Guid pollId) { }

        public void OptionAddedEvent(Option option, Guid pollId) { }
        public void OptionUpdatedEvent(Option option, Guid pollId) { }
        public void OptionDeletedEvent(Option option, Guid pollId) { }

        public void VoteAddedEvent(Vote vote, Guid pollId) { }
        public void VoteDeletedEvent(Vote vote, Guid pollId) { }

        public void BallotAddedEvent(Ballot ballot, Guid pollId) { }
        public void BallotDeletedEvent(Ballot ballot, Guid pollId) { }

        public void LoginEvent(string username) { }
        public void RegisterEvent(string username) { }
    }
}