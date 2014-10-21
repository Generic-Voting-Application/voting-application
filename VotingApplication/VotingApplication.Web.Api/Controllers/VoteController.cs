using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class VoteController : ApiController
    {
        private readonly IContextFactory _contextFactory;

        public VoteController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        #region Get
        public HttpResponseMessage Get()
        {
            using(var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Votes.ToList<Vote>());
            }
        }

        public HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Vote voteForId = context.Votes.Where(u => u.Id == id).FirstOrDefault();
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
    }
}