using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageController : WebApiController
    {
        public ManageController() : base() {}
        public ManageController(IContextFactory contextFactory) : base(contextFactory) { }

        private ManagePollRequestResponseModel PollToModel(Poll poll)
        {
            return new ManagePollRequestResponseModel
            {
                UUID = poll.UUID,
                Options = poll.Options,
            };
        }

        #region GET

        public override HttpResponseMessage Get()
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET on this controller");
        }

        public virtual HttpResponseMessage Get(Guid manageId)
        {
            #region DB Get / Validation
            Poll poll;
            using (var context = _contextFactory.CreateContext())
            {
                poll = context.Polls.Where(s => s.ManageId == manageId).Include(s => s.Options).FirstOrDefault();
            }

            if (poll == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll for {0} not found", manageId));
            }
            #endregion

            return this.Request.CreateResponse(HttpStatusCode.OK, PollToModel(poll));
        }


        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid manageId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(Guid manageId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid manageId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid manageId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by id on this controller");
        }

        #endregion
    }
}