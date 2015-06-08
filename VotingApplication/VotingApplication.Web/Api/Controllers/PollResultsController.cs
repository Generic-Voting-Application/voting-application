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
        public ResultsRequestResponseModel Get(ResultsRequestModel request)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByPollId(request.PollId, context);

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
                        _metricHandler.HandleResultsUpdateEvent(HttpStatusCode.NotModified, request.PollId);
                        throw new HttpResponseException(HttpStatusCode.NotModified);
                    }
                }

                List<Vote> votes = context
                    .Votes
                    .Include(v => v.Poll)
                    .Where(v => v.Poll.UUID == request.PollId)
                    .Include(v => v.Choice)
                    .Include(v => v.Ballot)
                    .ToList();

                List<VoteRequestResponseModel> responseVotes = votes
                    .Select(VoteToModel)
                    .ToList();

                _metricHandler.HandleResultsUpdateEvent(HttpStatusCode.OK, request.PollId);

                ResultsRequestResponseModel results = SummariseVotes(votes, poll);
                results.Votes = responseVotes;

                return results;
            }
        }

        private static ResultsRequestResponseModel SummariseVotes(List<Vote> votes, Poll poll)
        {
            ResultsRequestResponseModel summary = new ResultsRequestResponseModel();

            var results = from vote in votes
                          group vote by vote.Choice.PollChoiceNumber into result
                          let optionGroupVotes = result.ToList()
                          let optionGroupOption = optionGroupVotes[0].Choice
                          orderby optionGroupOption.PollChoiceNumber ascending
                          select new
                          {
                              Option = optionGroupOption,
                              Sum = optionGroupVotes.Sum(v => v.VoteValue),
                              Voters = optionGroupVotes.Select(v => CreateResultVoteModel(v, poll.NamedVoting))
                          };

            if (!results.Any())
            {
                return summary;
            }

            int resultsMax = results.Max(r => r.Sum);

            summary.Winners = results
                              .Where(r => r.Sum == resultsMax)
                              .Select(r => r.Option)
                              .ToList();

            summary.Results = results
                              .Select(r => ResultToModel(r.Option, r.Sum, r.Voters.ToList()))
                              .ToList();
            return summary;
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

        private static DateTime UnixTimeToDateTime(double unixTimestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimestamp);
            return dateTime;
        }

        private static ResultModel ResultToModel(Choice choice, int sum, List<ResultVoteModel> voters)
        {
            return new ResultModel
            {
                Choice = choice,
                Sum = sum,
                Voters = voters
            };
        }

        private static VoteRequestResponseModel VoteToModel(Vote vote)
        {
            var model = new VoteRequestResponseModel();

            if (vote.Choice != null)
            {
                model.VoterId = vote.Ballot.Id;
                model.ChoiceId = vote.Choice.Id;
                model.ChoiceName = vote.Choice.Name;
            }

            if (vote.Ballot.VoterName != null)
            {
                model.VoterName = vote.Ballot.VoterName;
            }

            model.VoteValue = vote.VoteValue;

            return model;
        }
    }
}