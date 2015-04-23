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

                if (poll.InviteOnly != updateRequest.InviteOnly)
                {
                    poll.InviteOnly = updateRequest.InviteOnly;
                    _metricHandler.InviteOnlyChangedEvent(poll.InviteOnly, poll.UUID);
                }

                if (poll.NamedVoting != updateRequest.NamedVoting)
                {
                    poll.NamedVoting = updateRequest.NamedVoting;
                    _metricHandler.NamedVotingChangedEvent(poll.NamedVoting, poll.UUID);
                }

                if (poll.OptionAdding != updateRequest.OptionAdding)
                {
                    poll.OptionAdding = updateRequest.OptionAdding;
                    _metricHandler.OptionAddingChangedEvent(poll.OptionAdding, poll.UUID);
                }

                if (poll.HiddenResults != updateRequest.HiddenResults)
                {
                    poll.HiddenResults = updateRequest.HiddenResults;
                    _metricHandler.HiddenResultsChangedEvent(poll.HiddenResults, poll.UUID);
                }

                poll.LastUpdated = DateTime.Now;

                context.SaveChanges();
            }
        }
    }
}
