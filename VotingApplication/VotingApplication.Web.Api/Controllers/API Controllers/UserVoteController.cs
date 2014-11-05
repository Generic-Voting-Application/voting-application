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
        public UserVoteController() : base() {}
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

        public HttpResponseMessage Put(long userId, Vote vote)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (vote.OptionId == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Vote does not have an option");
                }

                if (vote.SessionId == Guid.Empty)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Vote does not have a session");
                }

                IEnumerable<User> users = context.Users.Where(u => u.Id == userId);
                if(users.Count() == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("User {0} does not exist", userId));
                }

                IEnumerable<Option> options = context.Options.Where(o => o.Id == vote.OptionId);
                if (options.Count() == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Option {0} does not exist", vote.OptionId));
                }

                IEnumerable<Session> sessions = context.Sessions.Where(o => o.UUID == vote.SessionId);
                if (sessions.Count() == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Session {0} does not exist", vote.SessionId));
                }

                IEnumerable<Vote> votes = context.Votes.Where(v => v.UserId == userId && v.SessionId == vote.SessionId);
                if(votes.Count() == 0)
                {
                    vote.UserId = userId;
                    context.Votes.Add(vote);
                    context.SaveChanges();
                    return this.Request.CreateResponse(HttpStatusCode.OK, vote.Id);

                }
                else
                {
                    Vote oldVote = votes.FirstOrDefault();
                    oldVote.OptionId = vote.OptionId;
                    context.SaveChanges();
                    return this.Request.CreateResponse(HttpStatusCode.OK, oldVote.Id);
                }

            }
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion
    }
}