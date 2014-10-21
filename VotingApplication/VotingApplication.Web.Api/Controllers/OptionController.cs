using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class OptionController : ApiController
    {
        private readonly IContextFactory _contextFactory;

        public OptionController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        #region Get
        public HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Options.ToList<Option>());
            }
        }

        public HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Option optionForId = context.Options.Where(u => u.Id == id).FirstOrDefault();
                if (optionForId == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound, (Option)null);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, optionForId);
                }
            }
        }
        #endregion
    }
}