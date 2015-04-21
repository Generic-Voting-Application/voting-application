using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageMiscController : WebApiController
    {
        public ManageMiscController() : base() { }

        public ManageMiscController(IContextFactory contextFactory, IMetricEventHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpPut]
        public void Put(Guid manageId, ManagePollMiscRequest updateRequest)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                poll.InviteOnly = updateRequest.InviteOnly;
                poll.NamedVoting = updateRequest.NamedVoting;
                poll.OptionAdding = updateRequest.OptionAdding;

                poll.LastUpdated = DateTime.Now;

                context.SaveChanges();
            }
        }
    }
}
