using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserController : ApiController
    {
        #region Get
        public IEnumerable<User> Get()
        {
            using (var context = new VotingContext())
            {
                return context.Users.ToList<User>();
            }
        }

        public User Get(long id)
        {
            throw new System.NotImplementedException();
        } 
        #endregion 
    }
}