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

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollResultsController : WebApiController
    {
        public PollResultsController() : base() { }

        public PollResultsController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        public ResultsRequestResponseModel Get(Guid pollId)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByPollId(pollId, context);

                if (Request.RequestUri != null)
                {
                    NameValueCollection queryMap = HttpUtility.ParseQueryString(Request.RequestUri.Query);
                    string lastRefreshedDate = queryMap["lastRefreshed"];

                    var clientLastUpdated = DateTime.MinValue;

                    if (lastRefreshedDate != null)
                    {
                        clientLastUpdated = UnixTimeToDateTime(long.Parse(lastRefreshedDate));
                    }

                    if (poll.LastUpdated < clientLastUpdated)
                    {
                        _metricHandler.ResultsUpdateEvent(HttpStatusCode.NotModified, pollId);
                        throw new HttpResponseException(HttpStatusCode.NotModified);
                    }
                }

                List<Vote> votes = context
                    .Votes
                    .Include(v => v.Poll)
                    .Where(v => v.Poll.UUID == pollId)
                    .Include(v => v.Option)
                    .Include(v => v.Ballot)
                    .ToList();

                List<VoteRequestResponseModel> responseVotes = votes
                    .Select(v => VoteToModel(v, poll))
                    .ToList();

                _metricHandler.ResultsUpdateEvent(HttpStatusCode.OK, pollId);

                ResultsRequestResponseModel results = SummariseVotes(votes);
                results.Votes = responseVotes;

                return results;
            }
        }

        private ResultsRequestResponseModel SummariseVotes(List<Vote> votes)
        {
            ResultsRequestResponseModel summary = new ResultsRequestResponseModel();

            var results = from vote in votes
                          group vote by vote.Option.PollOptionNumber into result
                          let optionGroupVotes = result.ToList()
                          let optionGroupOption = optionGroupVotes[0].Option
                          orderby optionGroupOption.PollOptionNumber ascending
                          select new
                          {
                              Option = optionGroupOption,
                              Sum = optionGroupVotes.Sum(v => v.VoteValue),
                              Voters = optionGroupVotes.Select(v => (new ResultVoteModel { Name = v.Ballot.VoterName, Value = v.VoteValue }))
                          };

            if (results.Count() == 0)
            {
                return summary;
            }

            int resultsMax = results.Max(r => r.Sum);

            summary.Winners = results.
                              Where(r => r.Sum == resultsMax).
                              Select(r => r.Option).
                              ToList();

            summary.Results = results.
                              Select(r => ResultToModel(r.Option, r.Sum, r.Voters.ToList<ResultVoteModel>())).
                              ToList();
            return summary;
        }

        private static DateTime UnixTimeToDateTime(double unixTimestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimestamp);
            return dateTime;
        }

        private ResultModel ResultToModel(Option option, int sum, List<ResultVoteModel> voters)
        {
            return new ResultModel
            {
                Option = option,
                Sum = sum,
                Voters = voters
            };
        }

        private VoteRequestResponseModel VoteToModel(Vote vote, Poll poll)
        {
            var model = new VoteRequestResponseModel();

            if (vote.Option != null)
            {
                model.VoterId = vote.Ballot.Id;
                model.OptionId = vote.Option.Id;
                model.OptionName = vote.Option.Name;
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