using System;
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

        public ManageMiscController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

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
                    _metricHandler.HandleInviteOnlyChangedEvent(poll.InviteOnly, poll.UUID);
                }

                if (poll.NamedVoting != updateRequest.NamedVoting)
                {
                    poll.NamedVoting = updateRequest.NamedVoting;
                    _metricHandler.HandleNamedVotingChangedEvent(poll.NamedVoting, poll.UUID);
                }

                if (poll.ChoiceAdding != updateRequest.ChoiceAdding)
                {
                    poll.ChoiceAdding = updateRequest.ChoiceAdding;
                    _metricHandler.HandleChoiceAddingChangedEvent(poll.ChoiceAdding, poll.UUID);
                }

                if (poll.IsElectionMode != updateRequest.IsElectionMode)
                {
                    poll.IsElectionMode = updateRequest.IsElectionMode;
                    _metricHandler.HandleIsElectionModeChangedEvent(poll.IsElectionMode, poll.UUID);
                }

                poll.LastUpdatedUtc = DateTime.UtcNow;

                context.SaveChanges();
            }
        }
    }
}
