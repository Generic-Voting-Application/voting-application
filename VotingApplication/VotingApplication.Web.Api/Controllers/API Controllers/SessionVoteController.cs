using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class SessionVoteController : WebApiController
    {
        public SessionVoteController() : base() {}
        public SessionVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public override HttpResponseMessage Get(long sessionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session session = context.Sessions.Where(s => s.Id == sessionId).FirstOrDefault();
                if (session == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                List<Vote> sessionVotes = context.Votes.Where(v => v.SessionId == sessionId)
                    .Include(v => v.Option).Include(v => v.User)
                    .ToList();
                return this.Request.CreateResponse(HttpStatusCode.OK, sessionVotes);
            }
        }

        public virtual HttpResponseMessage Get(long sessionId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(long sessionId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long sessionId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(long sessionId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(long sessionId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public override HttpResponseMessage Delete(long sessionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.Id == sessionId).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                List<Vote> votesInSession = context.Votes.Where(v => v.SessionId == sessionId).ToList();
                foreach (Vote vote in votesInSession)
                {
                    context.Votes.Remove(vote);
                }

                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        public virtual HttpResponseMessage Delete(long sessionId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.Id == sessionId).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                Vote matchingVote = context.Votes.Where(v => v.Id == voteId && v.SessionId == sessionId).FirstOrDefault();
                context.Votes.Remove(matchingVote);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion

    }
}