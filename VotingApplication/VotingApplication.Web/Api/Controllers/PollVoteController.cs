using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollVoteController : WebApiController
    {
        private readonly IVoteValidatorFactory _voteValidatorFactory;
        private const string ValidVoterNameRegex = @"[^\w\.@ -]";

        public PollVoteController() { }

        public PollVoteController(IContextFactory contextFactory, IMetricHandler metricHandler, IVoteValidatorFactory voteValidatorFactory)
            : base(contextFactory, metricHandler)
        {
            _voteValidatorFactory = voteValidatorFactory;
        }

        [HttpGet]
        public List<VoteRequestResponseModel> Get(Guid pollId, Guid tokenGuid)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByPollId(pollId, context);

                List<Vote> votes = context
                    .Votes
                    .Include(v => v.Poll)
                    .Where(v => v.Ballot.TokenGuid == tokenGuid && v.Poll.UUID == pollId)
                    .Include(v => v.Choice)
                    .Include(v => v.Ballot)
                    .ToList();

                return votes
                    .Select(v => VoteToModel(v, poll))
                    .ToList();
            }
        }

        private static VoteRequestResponseModel VoteToModel(Vote vote, Poll poll)
        {
            var model = new VoteRequestResponseModel();

            if (vote.Choice != null)
            {
                model.ChoiceId = vote.Choice.Id;
                model.ChoiceName = vote.Choice.Name;
            }

            if (poll.NamedVoting && vote.Ballot.VoterName != null)
            {
                model.VoterName = vote.Ballot.VoterName;
            }

            model.VoteValue = vote.VoteValue;

            return model;
        }

        [HttpPut]
        public void Put(Guid pollId, Guid tokenGuid, BallotRequestModel ballotRequest)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                if (ballotRequest == null)
                {
                    ThrowError(HttpStatusCode.BadRequest);
                }

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                Poll poll = context
                    .Polls
                    .Where(p => p.UUID == pollId)
                    .Include(p => p.Ballots)
                    .Include(p => p.Choices)
                    .SingleOrDefault();

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, String.Format("Poll {0} not found", pollId));
                }

                if (poll.ExpiryDateUtc.HasValue && poll.ExpiryDateUtc < DateTime.UtcNow)
                {
                    ThrowError(HttpStatusCode.Forbidden, String.Format("Poll {0} has expired", pollId));
                }


                foreach (VoteRequestModel voteRequest in ballotRequest.Votes)
                {
                    if (poll.Choices.All(o => o.Id != voteRequest.ChoiceId))
                    {
                        ModelState.AddModelError("OptionId", "Option choice not valid for this poll");
                    }
                }

                // Poll specific validation
                IVoteValidator voteValidator = _voteValidatorFactory.CreateValidator(poll.PollType);
                voteValidator.Validate(ballotRequest.Votes, poll, ModelState);

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }


                Ballot ballot = poll
                    .Ballots
                    .SingleOrDefault(t => t.TokenGuid == tokenGuid);

                if (ballot == null)
                {
                    if (poll.InviteOnly)
                    {
                        ThrowError(HttpStatusCode.Forbidden, String.Format("Token {0} not valid for this poll", tokenGuid));
                    }
                    else
                    {
                        ballot = new Ballot() { TokenGuid = tokenGuid };
                        poll.Ballots.Add(ballot);
                    }
                }

                List<Vote> existingVotes = context
                    .Votes
                    .Include(v => v.Poll)
                    .Where(v => v.Ballot.TokenGuid == tokenGuid && v.Poll.UUID == pollId)
                    .ToList();

                if (poll.IsElectionMode && existingVotes.Count > 0)
                {
                    ThrowError(HttpStatusCode.BadRequest, "Votes for this poll can not be changed");
                }

                foreach (Vote contextVote in existingVotes)
                {
                    _metricHandler.HandleVoteDeletedEvent(contextVote, pollId);
                    context.Votes.Remove(contextVote);
                }

                // For some reason, we don't have an addrange function on Entity Framework
                foreach (VoteRequestModel voteRequest in ballotRequest.Votes)
                {
                    Choice option = context
                        .Choices
                        .Single(o => o.Id == voteRequest.ChoiceId);

                    Vote modelToVote = ModelToVote(voteRequest, ballot, option, poll);
                    context.Votes.Add(modelToVote);

                    _metricHandler.HandleVoteAddedEvent(modelToVote, pollId);
                }

                if (!String.IsNullOrEmpty(ballotRequest.VoterName))
                {
                    ballot.VoterName = Regex.Replace(ballotRequest.VoterName, ValidVoterNameRegex, "");
                }

                poll.LastUpdatedUtc = DateTime.UtcNow;

                context.SaveChanges();
            }
        }

        private static Vote ModelToVote(VoteRequestModel voteRequest, Ballot ballot, Choice option, Poll poll)
        {
            return new Vote
            {
                Choice = option,
                Poll = poll,
                Ballot = ballot,
                VoteValue = voteRequest.VoteValue
            };

        }
    }
}