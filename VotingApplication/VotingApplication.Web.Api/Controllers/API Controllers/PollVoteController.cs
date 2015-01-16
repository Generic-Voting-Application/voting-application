using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
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
                model.OptionId = vote.Option.Id;
                model.OptionName = vote.Option.Name;
            }

            if (vote.User != null)
            {
                model.VoterName = poll.AnonymousVoting ? "Anonymous User" : vote.User.Name;
                model.UserId = vote.User.Id;
            }

            model.VoteValue = vote.PollValue;

            return model;
        }

        #region GET

        public virtual HttpResponseMessage Get(Guid pollId)
        {
            #region DBGet / Validation

            Poll poll;
            List<Vote> votes;

            using (var context = _contextFactory.CreateContext())
            {
                poll = context.Polls.Where(s => s.UUID == pollId).FirstOrDefault();
                votes = context.Votes.Where(v => v.PollId == pollId)
                .Include(v => v.Option).Include(v => v.User)
                .ToList();
            }

            if (poll == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
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
                    return this.Request.CreateResponse(HttpStatusCode.NotModified);
                }
            }

            #endregion

            return this.Request.CreateResponse(HttpStatusCode.OK, votes.Select(v => VoteToModel(v, poll)).ToList());
        }

        public virtual HttpResponseMessage Get(Guid pollId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        private static DateTime UnixTimeToDateTime(double unixTimestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimestamp);
            return dateTime;
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid pollId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(Guid pollId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid pollId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid pollId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(Guid pollId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by id on this controller");
        }

        #endregion

    }
}