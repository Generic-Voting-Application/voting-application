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

        public Guid Get(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.UUID == pollId).Include(s => s.Options).SingleOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
                }

                if (poll.InviteOnly)
                {
                    this.ThrowError(HttpStatusCode.Forbidden, string.Format("Poll {0} is invite only", pollId));
                }

                Guid newTokenGuid = Guid.NewGuid();

                if(poll.Tokens == null)
                {
                    poll.Tokens = new List<Ballot>();
                }

                poll.Tokens.Add(new Ballot { TokenGuid = newTokenGuid });

                context.SaveChanges();

                return newTokenGuid;
            }
        }

        #endregion

    }
}