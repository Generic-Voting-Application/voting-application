using System.Linq;
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

        public override HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.OptionSets.ToList<OptionSet>());
            }
        }

        public virtual HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                OptionSet matchingOptionSet = context.OptionSets.Where(os => os.Id == id).FirstOrDefault();
                if (matchingOptionSet == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("OptionSet {0} does not exist", id));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingOptionSet);
            }
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