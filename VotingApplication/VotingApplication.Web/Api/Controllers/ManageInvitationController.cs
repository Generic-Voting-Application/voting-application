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
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                return matchingPoll.Ballots
                    .Where(b => !String.IsNullOrWhiteSpace(b.Email))
                    .Select(BallotToModel)
                    .ToList();
            }
        }

        #endregion

        #region POST

        public void Post(Guid manageId, List<ManageInvitationRequestModel> invitees)
        {
            ValidateRequest(invitees);

            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = GetPoll(context, manageId);

                List<Ballot> redundantBallots = matchingPoll.Ballots.ToList<Ballot>();

                foreach (ManageInvitationRequestModel invitee in invitees)
                {
                    Ballot matchingBallot = matchingPoll.Ballots.SingleOrDefault(b => b.ManageGuid != Guid.Empty && b.ManageGuid == invitee.ManageToken);
                    redundantBallots.RemoveAll(b => b == matchingBallot);

                    if (matchingBallot == null)
                    {
                        matchingBallot = new Ballot() { Email = invitee.Email, ManageGuid = Guid.NewGuid() };
                        matchingPoll.Ballots.Add(matchingBallot);
                        context.Ballots.Add(matchingBallot);
                    }

                    if (invitee.SendInvitation)
                    {
                        SendInvitation(matchingBallot, matchingPoll);
                    }
                }

                redundantBallots.ForEach(b => DeleteBallot(context, b, matchingPoll));

                context.SaveChanges();
            }
        }

        private void ValidateRequest(List<ManageInvitationRequestModel> request)
        {
            if (request == null)
            {
                ThrowError(HttpStatusCode.BadRequest, "List of invitees cannot be null");
            }
        }

        private Poll GetPoll(IVotingContext context, Guid manageId)
        {
            Poll matchingPoll = context.Polls
                                        .Where(p => p.ManageId == manageId)
                                        .Include(p => p.Ballots.Select(b => b.Votes))
                                        .FirstOrDefault();

            if (matchingPoll == null)
            {
                ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
            }

            return matchingPoll;
        }

        private void SendInvitation(Ballot ballot, Poll poll)
        {
            if (ballot.TokenGuid == Guid.Empty)
            {
                ballot.TokenGuid = Guid.NewGuid();
            }

            _invitationService.SendInvitation(poll.UUID, ballot, poll.Name);
        }

        private void DeleteBallot(IVotingContext context, Ballot ballot, Poll poll)
        {
            List<Vote> redundantVotes = ballot.Votes.ToList();

            foreach (Vote redundantVote in redundantVotes)
            {
                context.Votes.Remove(redundantVote);
            }

            poll.Ballots.Remove(ballot);
            context.Ballots.Remove(ballot);

        }

        #endregion

    }
}