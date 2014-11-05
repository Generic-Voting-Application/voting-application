using System.Collections.Generic;
using System.Data.Entity;
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
                return this.Request.CreateResponse(HttpStatusCode.OK, context.OptionSets.Include(s => s.Options).ToList<OptionSet>());
            }
        }

        public virtual HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                OptionSet matchingOptionSet = context.OptionSets.Where(os => os.Id == id).Include(s => s.Options).FirstOrDefault();
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
            using (var context = _contextFactory.CreateContext())
            {
                if (newOptionSet.Name == null || newOptionSet.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "OptionSet does not have a name");
                }

                if (newOptionSet.Options == null)
                {
                    newOptionSet.Options = new List<Option>();
                }

                context.OptionSets.Add(newOptionSet);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newOptionSet.Id);
            }
        }

        #endregion
    }
}