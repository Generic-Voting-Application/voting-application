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

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollVoteController : WebApiController
    {
        public PollVoteController() : base() { }
        public PollVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.UUID == pollId).FirstOrDefault();
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
                        clientLastUpdated = DateTime.FromFileTimeUtc(long.Parse(lastPolledDate));
                    }

                    if (poll.LastUpdated.CompareTo(clientLastUpdated) < 0)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.NotModified);
                    }
                }

                List<Vote> pollVotes;

                if (poll.AnonymousVoting)
                {
                    pollVotes = context.Votes.Where(v => v.PollId == pollId)
                    .Include(v => v.Option).ToList();
                }
                else
                {
                    pollVotes = context.Votes.Where(v => v.PollId == pollId)
                    .Include(v => v.Option).Include(v => v.User)
                    .ToList();
                }


                return this.Request.CreateResponse(HttpStatusCode.OK, pollVotes);
            }
        }

        public virtual HttpResponseMessage Get(Guid pollId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
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