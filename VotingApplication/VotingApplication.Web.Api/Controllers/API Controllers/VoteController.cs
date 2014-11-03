using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Filters;

namespace VotingApplication.Web.Api.Controllers
{
    public class VoteController : WebApiController
    {
        public VoteController() : base() { }
        public VoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get
        public override HttpResponseMessage Get()
        {
            using(var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Votes.Include(v => v.Option).Include(v => v.User).ToList<Vote>());
            }
        }

        public override HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Vote voteForId = context.Votes.Where(v => v.Id == id).Include(v => v.Option).Include(v => v.User).FirstOrDefault();
                if (voteForId == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Vote {0} not found", id));
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, voteForId);
                }
            }
        } 
        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Vote newVote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion

        #region Delete
        [BasicAuthenticator(realm: "GVA")]
        public override HttpResponseMessage Delete()
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        [BasicAuthenticator(realm: "GVA")]
        public override HttpResponseMessage Delete(long id)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        #endregion
    }
}