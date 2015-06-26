using System;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Metrics
{
    public interface IMetricHandler
    {
        void HandlePageChangeEvent(string route, int statusCode, Guid pollId);
        void HandleErrorEvent(HttpResponseException exception, Guid pollId);

        void HandlePollCreatedEvent(Poll poll);
        void HandlePollClonedEvent(Poll poll);
        void HandleResultsUpdateEvent(HttpStatusCode status, Guid pollId);

        void HandleExpiryChangedEvent(DateTimeOffset? expiry, Guid pollId);
        void HandlePollTypeChangedEvent(PollType pollType, int maxPerVote, int maxPerPoll, Guid pollId);
        void HandleInviteOnlyChangedEvent(bool inviteOnly, Guid pollId);
        void HandleNamedVotingChangedEvent(bool namedVoting, Guid pollId);
        void HandleChoiceAddingChangedEvent(bool choiceAdding, Guid pollId);
        void HandleElectionModeChangedEvent(bool electionMode, Guid pollId);
        void HandleQuestionChangedEvent(string question, Guid pollId);

        void HandleChoiceAddedEvent(Choice choice, Guid pollId);
        void HandleChoiceUpdatedEvent(Choice choice, Guid pollId);
        void HandleChoiceDeletedEvent(Choice choice, Guid pollId);

        void HandleVoteAddedEvent(Vote vote, Guid pollId);
        void HandleVoteDeletedEvent(Vote vote, Guid pollId);

        void HandleBallotAddedEvent(Ballot ballot, Guid pollId);
        void HandleBallotDeletedEvent(Ballot ballot, Guid pollId);

        void HandleLoginEvent(string username);
        void HandleRegisterEvent(string username);
    }
}