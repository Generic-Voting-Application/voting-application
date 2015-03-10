using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Description;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollVoteController : WebApiController
    {
        public PollVoteController() : base() { }
        public PollVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        private VoteRequestResponseModel VoteToModel(Vote vote, Poll poll)
        {
            VoteRequestResponseModel model = new VoteRequestResponseModel();

            if (vote.Option != null)
            {
                model.VoterId = vote.Token.Id;
                model.OptionId = vote.Option.Id;
                model.OptionName = vote.Option.Name;
            }

            model.VoterName = poll.NamedVoting ? vote.VoterName : "Anonymous User";
            model.VoteValue = vote.VoteValue;

            return model;
        }

        #region GET

        [ResponseType(typeof(IEnumerable<VoteRequestResponseModel>))]
        public HttpResponseMessage Get(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.UUID == pollId).FirstOrDefault();
                List<Vote> votes = context.Votes.Where(v => v.PollId == pollId).Include(v => v.Option).Include(v => v.Token).ToList();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
                }

                DateTime clientLastUpdated = DateTime.MinValue;
                if (this.Request.RequestUri != null && this.Request.RequestUri.Query != null)
                {
                    NameValueCollection queryMap = HttpUtility.ParseQueryString(this.Request.RequestUri.Query);
                    string lastPolledDate = queryMap["lastPoll"];

                    if (lastPolledDate != null)
                    {
                        clientLastUpdated = UnixTimeToDateTime(long.Parse(lastPolledDate));
                    }

                    if (poll.LastUpdated.CompareTo(clientLastUpdated) < 0)
                    {
                        return new HttpResponseMessage(HttpStatusCode.NotModified);
                    }
                }

                var result = votes.Select(v => VoteToModel(v, poll)).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }

        private static DateTime UnixTimeToDateTime(double unixTimestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimestamp);
            return dateTime;
        }

        #endregion
    }
}