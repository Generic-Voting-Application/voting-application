using System;
using System.Net;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public interface IMetricEventHandler
    {
        void PageChangeEvent(string route, int statusCode, Guid pollId);
        void ErrorEvent(HttpResponseException exception, Guid pollId);

        void PollCreatedEvent(Poll poll);
        void ResultsUpdateEvent(HttpStatusCode status, Guid pollId);

        void ExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId);
        void PollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId);
        void InviteOnlyChangedEvent(bool inviteOnly, Guid pollId);
        void NamedVotingChangedEvent(bool namedVoting, Guid pollId);
        void OptionAddingChangedEvent(bool optionAdding, Guid pollId);
        void HiddenResultsChangedEvent(bool hiddenResults, Guid pollId);
        void QuestionChangedEvent(string question, Guid pollId);

        void OptionAddedEvent(Option option, Guid pollId);
        void OptionUpdatedEvent(Option option, Guid pollId);
        void OptionDeletedEvent(Option option, Guid pollId);

        void VoteAddedEvent(Vote vote, Guid pollId);
        void VoteDeletedEvent(Vote vote, Guid pollId);

        void BallotAddedEvent(Ballot ballot, Guid pollId);
        void BallotDeletedEvent(Ballot ballot, Guid pollId);

        void LoginEvent();
        void RegisterEvent();
    }
}