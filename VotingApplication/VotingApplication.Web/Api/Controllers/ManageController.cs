using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageController : WebApiController
    {

        public ManageController() : base() { }

        public ManageController(IContextFactory contextFactory) : base(contextFactory) { }

        [HttpGet]
        [Authorize]
        [AllowAnonymous]
        public ManagePollRequestResponseModel Get(Guid manageId)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls
                    .Where(p => p.ManageId == manageId)
                    .Include(p => p.Options)
                    .Include(p => p.Ballots)
                    .Include(p => p.Ballots.Select(b => b.Votes))
                    .FirstOrDefault();

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                if (String.IsNullOrEmpty(poll.CreatorIdentity) && !String.IsNullOrEmpty(User.Identity.GetUserId()))
                {
                    poll.CreatorIdentity = User.Identity.GetUserId();
                    poll.Creator = User.Identity.GetUserName();
                    context.SaveChanges();
                }

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