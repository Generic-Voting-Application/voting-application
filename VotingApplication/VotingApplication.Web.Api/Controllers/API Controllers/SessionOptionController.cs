using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Filters;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class SessionOptionController : WebApiController
    {
        public SessionOptionController() : base() { }
        public SessionOptionController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(Guid sessionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.UUID == sessionId).Include(s => s.Options).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingSession.Options);
            }
        }

        public virtual HttpResponseMessage Get(Guid sessionId, long optionId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid sessionId, Option option)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (option.Name == null || option.Name == "")
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot create an option with a non-empty name");
                }

                Session matchingSession = context.Sessions.Where(s => s.UUID == sessionId).Include(s => s.Options).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                if (option.Sessions == null)
                {
                    option.Sessions = new List<Session>();
                }

                option.Sessions.Add(matchingSession);
                matchingSession.Options.Add(option);

                context.Options.Add(option);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, option.Id);
            }
        }

        public virtual HttpResponseMessage Post(Guid sessionId, long optionId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid sessionId, Option option)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid sessionId, long optionId, Option option)
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
        public virtual HttpResponseMessage Delete(Guid sessionId, long optionId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.UUID == sessionId).Include(s => s.Options).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", sessionId));
                }

                Option matchingOption = matchingSession.Options.Where(o => o.Id == optionId).FirstOrDefault();
                if (matchingOption != null)
                {
                    matchingSession.Options.Remove(matchingOption);
                }

                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion

    }
}