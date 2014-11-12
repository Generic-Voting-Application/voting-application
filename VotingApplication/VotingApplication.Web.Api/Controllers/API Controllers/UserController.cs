using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserController : WebApiController
    {
        public UserController() : base() { }
        public UserController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get
        public override HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Users.ToList<User>());
            }
        }

        public virtual HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                User userForId = context.Users.Where(u => u.Id == id).FirstOrDefault();
                if (userForId == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} not found", id));
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, userForId);
                }
            }
        } 
        #endregion 

        #region Put

        public HttpResponseMessage Put(User newUser)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newUser.Name == null || newUser.Name.Equals(""))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Must provide a Username");
                }
                else if (new Regex(@"[^\w| ]").IsMatch(newUser.Name))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unacceptable Username");
                }

                List<User> existingUsers = context.Users.Where(u => u.Name == newUser.Name).ToList<User>();
                if (existingUsers.Count() != 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, existingUsers.FirstOrDefault().Id);
                }

                context.Users.Add(newUser);
                context.SaveChanges();
                return this.Request.CreateResponse(HttpStatusCode.OK, newUser.Id);
            }
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long id, User newUser)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion
    }
}