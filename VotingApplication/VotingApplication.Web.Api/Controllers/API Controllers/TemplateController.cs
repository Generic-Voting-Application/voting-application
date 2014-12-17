using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class TemplateController : WebApiController
    {
        public TemplateController() : base() { }
        public TemplateController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get

        public override HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Templates.Include(s => s.Options).ToList<Template>());
            }
        }

        public virtual HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Template matchingTemplate = context.Templates.Where(os => os.Id == id).Include(s => s.Options).FirstOrDefault();
                if (matchingTemplate == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Template {0} not found", id));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingTemplate);
            }
        }

        #endregion

        #region Put

        public virtual HttpResponseMessage Put(Template newTemplate)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(Template newTemplate)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newTemplate.Name == null || newTemplate.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Template name must not be empty");
                }

                if (newTemplate.Options == null)
                {
                    newTemplate.Options = new List<Option>();
                }

                context.Templates.Add(newTemplate);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newTemplate.Id);
            }
        }

        public virtual HttpResponseMessage Post(long id, Template newTemplate)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion
    }
}