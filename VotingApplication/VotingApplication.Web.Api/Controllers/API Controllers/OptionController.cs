using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Filters;

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

        public virtual HttpResponseMessage Get(long id)
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

        #region Put

        public virtual HttpResponseMessage Put(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region Post

        public HttpResponseMessage Post()
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }


        public virtual HttpResponseMessage Post(long id, Option newOption)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region Delete
        [BasicAuthenticator(realm: "GVA")]
        public override HttpResponseMessage Delete(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Option option = context.Options.Where(o => o.Id == id).FirstOrDefault();
                if (option != null)
                {
                    context.Options.Remove(option);
                    context.SaveChanges();
                }

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion
    }
}