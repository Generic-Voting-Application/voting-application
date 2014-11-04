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
    public class SessionController : WebApiController
    {
        public SessionController() : base() { }
        public SessionController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get

        public override HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Sessions.ToList());
            }
        }

        public override HttpResponseMessage Get(long sessionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.Id == sessionId).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingSession);
            }
        }

        #endregion

        #region Put

        public virtual HttpResponseMessage Put(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(Session newSession)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newSession.Name == null || newSession.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Session did not have a name");
                }

                newSession.UUID = Guid.NewGuid();

                context.Sessions.Add(newSession);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newSession.UUID);
            }
        }

        #endregion
    }
}