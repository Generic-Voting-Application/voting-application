using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserController : ApiController
    {
        private readonly IContextFactory _contextFactory;

        public UserController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        #region Get
        public HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Users.ToList<User>());
            }
        }

        public User Get(long id)
        {
            throw new System.NotImplementedException();
        } 
        #endregion 
    }
}