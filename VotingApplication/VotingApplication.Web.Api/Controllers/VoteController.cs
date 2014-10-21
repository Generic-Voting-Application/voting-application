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

        public Vote Get(long id)
        {
            throw new System.NotImplementedException();
        } 
        #endregion
    }
}