using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserVoteController : ApiController
    {
        private readonly IContextFactory _contextFactory;

        public UserVoteController()
        {
            this._contextFactory = new ContextFactory();
        }

        public UserVoteController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        #region GET

        public HttpResponseMessage Get(long userId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userExists = (context.Users.Where(u => u.Id == userId).Count() == 1);
                if (userExists)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, context.Votes.Where(v => v.UserId == userId).ToList<Vote>());
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
    }
}