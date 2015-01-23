using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Filters;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollTokenController : WebApiController
    {
        public PollTokenController() : base() { }
        public PollTokenController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.UUID == pollId).Include(s => s.Options).SingleOrDefault();

                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
                }

                if (poll.InviteOnly)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, string.Format("Poll {0} is invite only", pollId));
                }

                Guid newTokenGuid = Guid.NewGuid();

                if(poll.Tokens == null)
                {
                    poll.Tokens = new List<Token>();
                }

                poll.Tokens.Add(new Token { TokenGuid = newTokenGuid });

                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newTokenGuid);
            }
        }

        #endregion

        #region POST
        public virtual HttpResponseMessage Post(Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        #endregion

    }
}