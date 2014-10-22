using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class VoteController : ApiController
    {
        private readonly IContextFactory _contextFactory;

        public VoteController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        #region Get
        public IEnumerable<Vote> Get()
        {
            using(var context = _contextFactory.CreateContext())
            {
                return context.Votes.ToList<Vote>();
            }
        }

        public Vote Get(long id)
        {
            throw new System.NotImplementedException();
        } 
        #endregion
    }
}