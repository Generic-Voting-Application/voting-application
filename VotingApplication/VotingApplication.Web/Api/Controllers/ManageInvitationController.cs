using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageInvitationController : WebApiController
    {
        private ICorrespondenceService _correspondenceService;

        public ManageInvitationController(ICorrespondenceService correspondenceService)
            : base()
        {
            _correspondenceService = correspondenceService;
        }
        public ManageInvitationController(IContextFactory contextFactory, IMetricHandler metricHandler, ICorrespondenceService correspondenceService)
            : base(contextFactory, metricHandler)
        {
            _correspondenceService = correspondenceService;
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
                Poll matchingPoll = PollByManageId(manageId, context);

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
                Poll matchingPoll = PollByManageId(manageId, context);

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

        private void SendInvitation(Ballot ballot, Poll poll)
        {
            if (ballot.TokenGuid == Guid.Empty)
            {
                ballot.TokenGuid = Guid.NewGuid();
                _metricHandler.HandleBallotAddedEvent(ballot, poll.UUID);
            }

            _correspondenceService.SendInvitation(poll.UUID, ballot, poll.Name);
        }

        private void DeleteBallot(IVotingContext context, Ballot ballot, Poll poll)
        {
            List<Vote> redundantVotes = ballot.Votes.ToList();

            foreach (Vote redundantVote in redundantVotes)
            {
                _metricHandler.HandleVoteDeletedEvent(redundantVote, poll.UUID);
                context.Votes.Remove(redundantVote);
            }

            if (ballot.TokenGuid != Guid.Empty)
            {
                _metricHandler.HandleBallotDeletedEvent(ballot, poll.UUID);
            }

            poll.Ballots.Remove(ballot);
            context.Ballots.Remove(ballot);

        }

        #endregion

    }
}