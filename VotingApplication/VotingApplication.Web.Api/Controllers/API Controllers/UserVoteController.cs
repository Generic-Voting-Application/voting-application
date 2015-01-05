using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserVoteController : WebApiController
    {
        public UserVoteController() : base() { }
        public UserVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(long userId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userExists = (context.Users.Where(u => u.Id == userId).Count() == 1);
                if (userExists)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, context.Votes.Where(v => v.UserId == userId).Include(v => v.Option).Include(v => v.User).ToList<Vote>());
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} not found", userId));
                }
            }
        }

        public HttpResponseMessage Get(long userId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userVoteResponse = Get(userId);
                List<Vote> userVotes = ((ObjectContent)userVoteResponse.Content).Value as List<Vote>;
                if (userVotes == null)
                {
                    return userVoteResponse;
                }

                Vote matchingVote = userVotes.Where(v => v.Id == voteId).FirstOrDefault();
                if (matchingVote != null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, matchingVote);
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Vote {0} not found", voteId));
                }
            }

        }

        #endregion

        #region PUT

        public HttpResponseMessage Put(long userId, List<Vote> votes)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long id, Vote newUser)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion
    }
}