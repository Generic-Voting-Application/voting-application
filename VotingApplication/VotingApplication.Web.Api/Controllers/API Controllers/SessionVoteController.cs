using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Filters;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class SessionVoteController : WebApiController
    {
        public SessionVoteController() : base() {}
        public SessionVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(Guid sessionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session session = context.Sessions.Where(s => s.UUID == sessionId).FirstOrDefault();
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

        public virtual HttpResponseMessage Get(Guid sessionId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid sessionId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(Guid sessionId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid sessionId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid sessionId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(Guid sessionId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        [BasicAuthenticator(realm: "GVA")]
        public virtual HttpResponseMessage Delete(Guid sessionId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.UUID == sessionId).FirstOrDefault();
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