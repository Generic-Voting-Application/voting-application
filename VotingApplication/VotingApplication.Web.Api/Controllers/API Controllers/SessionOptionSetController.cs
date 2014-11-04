using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class SessionOptionSetController : WebApiController
    {
        public SessionOptionSetController() : base() { }
        public SessionOptionSetController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get

        public virtual HttpResponseMessage Get(Guid sessionId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET on this controller");
        }

        public virtual HttpResponseMessage Get(Guid sessionId, long optionSetId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by Id on this controller");
        }

        #endregion

        #region Put

        public virtual HttpResponseMessage Put(Guid sessionId, OptionSet newOptionSet)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public virtual HttpResponseMessage Put(Guid sessionId, long optionSetId, OptionSet newOptionSet)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by Id on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(Guid sessionId, OptionSet newOptionSet)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(Guid sessionId, long optionSetId, OptionSet newOptionSet)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by Id on this controller");
        }

        #endregion

        #region Delete

        public virtual HttpResponseMessage Delete(Guid sessionId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(Guid sessionId, long optionSetId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by Id on this controller");
        }

        #endregion
    }
}