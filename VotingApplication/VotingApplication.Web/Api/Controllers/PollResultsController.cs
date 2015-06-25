using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
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

                if (Request.RequestUri != null)
                {
                    NameValueCollection queryMap = HttpUtility.ParseQueryString(Request.RequestUri.Query);
                    string lastRefreshedDate = queryMap["lastRefreshed"];

                    var clientLastUpdated = DateTime.MinValue;

                    if (lastRefreshedDate != null)
                    {
                        clientLastUpdated = UnixTimeToDateTime(long.Parse(lastRefreshedDate));
                    }

                    if (poll.LastUpdatedUtc < clientLastUpdated)
                    {
                        _metricHandler.HandleResultsUpdateEvent(HttpStatusCode.NotModified, pollId);
                        throw new HttpResponseException(HttpStatusCode.NotModified);
                    }
                }

                List<Vote> votes = context
                    .Votes
                    .Include(v => v.Choice)
                    .Include(v => v.Ballot)
                    .Where(v => v.Poll.UUID == pollId)
                    .ToList();

                _metricHandler.HandleResultsUpdateEvent(HttpStatusCode.OK, pollId);

                return GenerateResults(votes, poll.NamedVoting);
            }
        }

        private static DateTime UnixTimeToDateTime(double unixTimestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimestamp);
            return dateTime;
        }

        private static ResultsRequestResponseModel GenerateResults(IEnumerable<Vote> votes, bool namedVoting)
        {
            var groupedResults = (from vote in votes
                                  group vote by vote.Choice.PollChoiceNumber into result
                                  let optionGroupVotes = result.ToList()
                                  select new
                                             {
                                                 ChoiceName = optionGroupVotes[0].Choice.Name,
                                                 Sum = optionGroupVotes.Sum(v => v.VoteValue),
                                                 Voters = optionGroupVotes.Select(v => CreateResultVoteModel(v, namedVoting)).ToList()
                                             })
                  .ToList();

            if (!groupedResults.Any())
            {
                return new ResultsRequestResponseModel();
            }

            int resultsMax = groupedResults.Max(r => r.Sum);

            List<string> winners = groupedResults
                .Where(r => r.Sum == resultsMax)
                .Select(r => r.ChoiceName)
                .ToList();


            List<ResultModel> resultModels = groupedResults
                .Select(r => ResultToModel(r.ChoiceName, r.Sum, r.Voters))
                .ToList();

            return new ResultsRequestResponseModel
            {
                Winners = winners,
                Results = resultModels
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

        private static ResultModel ResultToModel(string choiceName, int sum, List<ResultVoteModel> voters)
        {
            return new ResultModel
            {
                ChoiceName = choiceName,
                Sum = sum,
                Voters = voters
            };
        }
    }
}