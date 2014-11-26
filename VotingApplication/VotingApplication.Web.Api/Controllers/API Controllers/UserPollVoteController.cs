using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class UserPollVoteController : WebApiController
    {
        public UserPollVoteController() : base() {}
        public UserPollVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(long userId, Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                User matchingUser = context.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (matchingUser == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} does not exist", userId));
                }

                Poll matchingPoll = context.Polls.Where(s => s.UUID == pollId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", pollId));
                }

                IEnumerable<Vote> allVotesForUserInPoll = context.Votes.Where(v => v.UserId == userId && v.PollId == pollId);
                return this.Request.CreateResponse(HttpStatusCode.OK, allVotesForUserInPoll.ToList());
            }
        }

        public virtual HttpResponseMessage Get(long userId, Guid pollId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userPollResponse = Get(userId, pollId);
                List<Vote> votesForUserPoll = ((ObjectContent)userPollResponse.Content).Value as List<Vote>;
                if (votesForUserPoll == null)
                {
                    return userPollResponse;
                }

                Vote matchingVote = votesForUserPoll.Where(v => v.Id == voteId).FirstOrDefault();
                if (matchingVote == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Vote {0} does not exist", voteId));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingVote);
            }
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(long userId, Guid pollId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long userId, Guid pollId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(long userId, Guid pollId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(long userId, Guid pollId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(long userId, Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(long userId, Guid pollId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by id on this controller");
        }

        #endregion

    }
}