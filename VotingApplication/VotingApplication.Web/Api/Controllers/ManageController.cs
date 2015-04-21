using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageController : WebApiController
    {

        public ManageController() : base() { }

        public ManageController(IContextFactory contextFactory, IMetricEventHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        [Authorize]
        [AllowAnonymous]
        public ManagePollRequestResponseModel Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);
                return CreateResponseModelFromPoll(poll);
            }
        }

        private ManagePollRequestResponseModel CreateResponseModelFromPoll(Poll poll)
        {
            List<ManageInvitationResponseModel> Voters = poll
                .Ballots
                .Where(b => b.Votes.Any())
                .Select(CreateManagePollRequestResponseModel)
                .ToList();

            List<Ballot> Invitees = poll
                .Ballots
                .Where(b => !String.IsNullOrWhiteSpace(b.Email))
                .ToList<Ballot>();

            return new ManagePollRequestResponseModel
            {
                UUID = poll.UUID,
                Options = poll.Options,
                InviteeCount = Invitees.Count,
                VotersCount = Voters.Count,
                VotingStrategy = poll.PollType.ToString(),
                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,
                InviteOnly = poll.InviteOnly,
                Name = poll.Name,
                NamedVoting = poll.NamedVoting,
                ExpiryDate = poll.ExpiryDate,
                OptionAdding = poll.OptionAdding
            };
        }

        private ManageInvitationResponseModel CreateManagePollRequestResponseModel(Ballot ballot)
        {
            return new ManageInvitationResponseModel
            {
                Email = ballot.Email,
                EmailSent = (ballot.TokenGuid != null && ballot.TokenGuid != Guid.Empty),
                ManageToken = ballot.ManageGuid
            };
        }
    }
}