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

        private ManageInvitationResponseModel BallotToModel(Ballot ballot)
        {
            return new ManageInvitationResponseModel
            {
                Email = ballot.Email,
                EmailSent = (ballot.TokenGuid != null && ballot.TokenGuid != Guid.Empty),
                ManageToken = ballot.ManageGuid
            };
        }

        #region GET

        public List<ManageInvitationResponseModel> Get(Guid manageId)
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

                return matchingPoll.Ballots
                    .Where(b => !String.IsNullOrWhiteSpace(b.Email))
                    .Select(b => BallotToModel(b))
                    .ToList<ManageInvitationResponseModel>();
            }
        }

        #endregion

        #region POST

        public void Post(Guid manageId, List<ManageInvitationRequestModel> invitees)
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

                List<Ballot> redundantBallots = matchingPoll.Ballots.ToList<Ballot>();

                foreach (ManageInvitationRequestModel invitee in invitees)
                {
                    Ballot matchingBallot = matchingPoll.Ballots.SingleOrDefault(b => b.ManageGuid == invitee.ManageToken);

                    if (matchingBallot == null)
                    {
                        matchingBallot = new Ballot() { Email = invitee.Email, ManageGuid = Guid.NewGuid() };
                        matchingPoll.Ballots.Add(matchingBallot);
                    }
                    else
                    {
                        redundantBallots.Remove(matchingBallot);
                    }

                    if (invitee.SendInvitation)
                    {
                        if (matchingBallot.TokenGuid == Guid.Empty)
                        {
                            matchingBallot.TokenGuid = Guid.NewGuid();
                        }

                        _invitationService.SendInvitation(matchingPoll.UUID, matchingBallot, matchingPoll.Name);
                    }
                }
                
                foreach (Ballot redundantBallot in redundantBallots)
                {
                    foreach (Vote redundantVote in redundantBallot.Votes)
                    {
                        context.Votes.Remove(redundantVote);
                    }

                    //context.Ballots.Remove(redundantBallot);
                    matchingPoll.Ballots.Remove(redundantBallot);
                }

                context.SaveChanges();
            }
        }

        #endregion

    }
}