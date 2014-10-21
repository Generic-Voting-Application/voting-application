using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class OptionController : ApiController
    {
        #region Get
        public IEnumerable<Option> Get()
        {
            using (var context = new VotingContext())
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