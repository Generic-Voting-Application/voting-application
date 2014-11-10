using System.Data.Entity;
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
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Sessions.Include(s => s.Options).ToList());
            }
        }

        public virtual HttpResponseMessage Get(Guid id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.UUID == id).Include(s => s.Options).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", id));
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

                if (newSession.Options == null)
                {
                    if (newSession.OptionSetId != 0)
                    {
                        newSession.Options = context.OptionSets.Where(os => os.Id == newSession.OptionSetId).Include(os => os.Options).FirstOrDefault().Options;
                    }
                    else
                    {
                        newSession.Options = new List<Option>();
                    }
                }

                newSession.UUID = Guid.NewGuid();

                context.Sessions.Add(newSession);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newSession.UUID);
            }
        }

        public virtual HttpResponseMessage Post(Guid id, Session newSession)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Session is null");
                }

                if (newSession.Name == null || newSession.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Session does not have a name");
                }

                if (newSession.Options == null)
                {
                    List<Option> options = new List<Option>();
                    if (newSession.OptionSetId != 0)
                    {
                        options = context.OptionSets.Where(os => os.Id == newSession.OptionSetId).Include(os => os.Options).FirstOrDefault().Options;
                    }

                    newSession.Options = options;
                }

                Session matchingSession = context.Sessions.Where(s => s.UUID == id).FirstOrDefault();
                if (matchingSession != null)
                {
                    context.Sessions.Remove(matchingSession);
                }

                context.Sessions.Add(newSession);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newSession.UUID);
            }
        }

        #endregion
    }
}