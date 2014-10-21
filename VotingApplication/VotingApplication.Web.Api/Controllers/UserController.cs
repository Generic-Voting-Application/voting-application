using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserController : ApiController
    {
        #region Get
        public HttpResponseMessage Get()
        {
            using (var context = new VotingContext())
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