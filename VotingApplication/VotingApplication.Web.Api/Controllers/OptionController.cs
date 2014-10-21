using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class OptionController : ApiController
    {
        private readonly IContextFactory _contextFactory;

        public OptionController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        #region Get
        public IEnumerable<Option> Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return context.Options.ToList<Option>();
            }
        }

        public Option Get(long id)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}