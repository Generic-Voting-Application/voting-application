using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class OptionSetController : WebApiController
    {
        public OptionSetController() : base() { }
        public OptionSetController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get

        public virtual HttpResponseMessage Get(long id)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by Id on this controller");
        }

        #endregion

        #region Put

        public virtual HttpResponseMessage Put(OptionSet newOptionSet)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(OptionSet newOptionSet)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion
    }
}