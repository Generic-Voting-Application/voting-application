using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class OptionController : WebApiController
    {
        public OptionController() : base() { }
        public OptionController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get
        public override HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Options.ToList<Option>());
            }
        }

        public override HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Option optionForId = context.Options.Where(u => u.Id == id).FirstOrDefault();
                if (optionForId == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Option {0} not found", id));
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, optionForId);
                }
            }
        }
        #endregion

        #region POST

        public virtual HttpResponseMessage Post(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion
    }
}