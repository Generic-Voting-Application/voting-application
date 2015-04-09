using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageInvitationController : WebApiController
    {
        private IInvitationService _invitationService;

        public ManageInvitationController(IInvitationService invitationService)
            : base()
        {
            _invitationService = invitationService;
        }
        public ManageInvitationController(IContextFactory contextFactory, IInvitationService invitationService)
            : base(contextFactory)
        {
            _invitationService = invitationService;
        }

        private ManagePollBallotRequestModel BallotToModel(Ballot ballot)
        {
            return new ManagePollBallotRequestModel
            {
                Email = ballot.Email,
                EmailSent = (ballot.TokenGuid != null && ballot.TokenGuid != Guid.Empty),
                ManageToken = ballot.ManageGuid
            };
        }

        #region GET

        public List<ManagePollBallotRequestModel> Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls
                                            .Where(p => p.ManageId == manageId)
                                            .Include(p => p.Ballots)
                                            .FirstOrDefault();

                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                return matchingPoll.Ballots.Select(b => BallotToModel(b)).ToList<ManagePollBallotRequestModel>();
            }
        }

        #endregion

        #region POST

        public void Post(Guid manageId, List<ManagePollBallotRequestModel> invitees)
        {
            if (invitees == null)
            {
                this.ThrowError(HttpStatusCode.BadRequest, "List of invitees cannot be null");
            }

            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls
                                        .Where(p => p.ManageId == manageId)
                                        .Include(p => p.Ballots)
                                        .FirstOrDefault();

                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                foreach (ManagePollBallotRequestModel invitee in invitees)
                {
                    Ballot matchingBallot = matchingPoll.Ballots.SingleOrDefault(b => b.Email != null && b.Email.Equals(invitee.Email, StringComparison.OrdinalIgnoreCase));

                    if (matchingBallot == null)
                    {
                        matchingBallot = new Ballot() { Email = invitee.Email, ManageGuid = Guid.NewGuid() };
                        matchingPoll.Ballots.Add(matchingBallot);
                    }

                    if (matchingBallot.TokenGuid == Guid.Empty)
                    {
                        matchingBallot.TokenGuid = Guid.NewGuid();
                    }

                    _invitationService.SendInvitation(matchingPoll.UUID, matchingBallot, matchingPoll.Name);
                }

                context.SaveChanges();
            }
        }

        #endregion

    }
}