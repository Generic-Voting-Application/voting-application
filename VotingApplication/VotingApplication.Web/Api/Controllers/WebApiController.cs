using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class WebApiController : ApiController
    {
        protected readonly IContextFactory _contextFactory;

        public WebApiController()
        {
            this._contextFactory = new ContextFactory();
        }

        public WebApiController(IContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }
    }
}