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
    public class UserSessionVoteController : WebApiController
    {
        public UserSessionVoteController() : base() {}
        public UserSessionVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(long userId, long sessionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                User matchingUser = context.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (matchingUser == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} does not exist", userId));
                }

                Session matchingSession = context.Sessions.Where(s => s.Id == sessionId).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                IEnumerable<Vote> allVotesForUserInSession = context.Votes.Where(v => v.UserId == userId && v.SessionId == sessionId);
                return this.Request.CreateResponse(HttpStatusCode.OK, allVotesForUserInSession.ToList());
            }
        }

        public virtual HttpResponseMessage Get(long userId, long sessionId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userSessionResponse = Get(userId, sessionId);
                List<Vote> votesForUserSession = ((ObjectContent)userSessionResponse.Content).Value as List<Vote>;
                if (votesForUserSession == null)
                {
                    return userSessionResponse;
                }

                Vote matchingVote = votesForUserSession.Where(v => v.Id == voteId).FirstOrDefault();
                if (matchingVote == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Vote {0} does not exist", voteId));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingVote);
            }
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(long userId, long sessionId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long userId, long sessionId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(long userId, long sessionId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(long userId, long sessionId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(long userId, long sessionId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(long userId, long sessionId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by id on this controller");
        }

        #endregion

    }
}