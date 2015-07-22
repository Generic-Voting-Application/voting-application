using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollResultsController : WebApiController
    {
        public PollResultsController()
        { }

        public PollResultsController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        public ResultsRequestResponseModel Get(Guid pollId)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByPollId(pollId, context);

                Guid? tokenGuid = GetTokenGuidFromHeaders();

                if (poll.InviteOnly)
                {
                    if (!tokenGuid.HasValue)
                    {
                        ThrowError(HttpStatusCode.Unauthorized);
                    }

                    if (poll.Ballots.All(b => b.TokenGuid != tokenGuid.Value))
                    {
                        ThrowError(HttpStatusCode.Unauthorized);
                    }
                }

                if (!tokenGuid.HasValue)
                {
                    if (poll.ElectionMode)
                    {
                        ThrowError(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    Ballot ballot = poll.Ballots.Where(b => b.TokenGuid == tokenGuid.Value).SingleOrDefault();

                    if (ballot == null)
                    {
                        ThrowError(HttpStatusCode.NotFound);
                    }

                    if (poll.ElectionMode && !ballot.HasVoted)
                    {
                        ThrowError(HttpStatusCode.Forbidden);
                    }
                }

                List<Vote> votes = context
                    .Votes
                    .Include(v => v.Choice)
                    .Include(v => v.Ballot)
                    .Where(v => v.Poll.UUID == pollId)
                    .ToList();

                _metricHandler.HandleResultsUpdateEvent(HttpStatusCode.OK, pollId);

                ResultsRequestResponseModel response = GenerateResults(votes, poll.Choices, poll.NamedVoting);
                response.PollName = poll.Name;
                response.NamedVoting = poll.NamedVoting;
                response.HasExpired = poll.ExpiryDateUtc.HasValue && poll.ExpiryDateUtc.Value < DateTime.UtcNow;
                return response;
            }
        }

        private static DateTime UnixTimeToDateTime(double unixTimestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimestamp);
            return dateTime;
        }

        private static ResultsRequestResponseModel GenerateResults(IEnumerable<Vote> votes, IEnumerable<Choice> choices, bool namedVoting)
        {
            List<Choice> pollChoices = choices.ToList();
            List<Vote> pollVotes = votes.ToList();

            if (!pollChoices.Any())
            {
                return new ResultsRequestResponseModel
                {
                    Winners = new List<string>(),
                    Results = new List<ResultModel>()
                };
            }

            var groupedResults = new List<ResultModel>();

            foreach (Choice choice in pollChoices)
            {
                IEnumerable<Vote> choiceVotes = pollVotes.Where(v => v.Choice.Id == choice.Id);

                var result = new ResultModel();
                result.ChoiceName = choice.Name;
                result.ChoiceNumber = choice.PollChoiceNumber;
                result.Voters = choiceVotes.Select(v => CreateResultVoteModel(v, namedVoting)).ToList();
                result.Sum = choiceVotes.Sum(v => v.VoteValue);

                groupedResults.Add(result);
            }

            int? resultsMax = groupedResults.Max(r => r.Sum);

            List<string> winners;

            if (resultsMax.HasValue && resultsMax > 0)
            {
                winners = groupedResults
                            .Where(r => r.Sum == resultsMax)
                            .Select(r => r.ChoiceName)
                            .ToList();
            }
            else
            {
                winners = new List<string>();
            }

            return new ResultsRequestResponseModel
            {
                Winners = winners,
                Results = groupedResults
            };
        }

        private static ResultVoteModel CreateResultVoteModel(Vote vote, bool namedVoting)
        {
            var resultVoteModel = new ResultVoteModel
            {
                Name = null,
                Value = vote.VoteValue
            };

            if (namedVoting)
            {
                resultVoteModel.Name = String.IsNullOrWhiteSpace(vote.Ballot.VoterName) ? "Anonymous Voter" : vote.Ballot.VoterName;
            }

            return resultVoteModel;
        }
    }
}