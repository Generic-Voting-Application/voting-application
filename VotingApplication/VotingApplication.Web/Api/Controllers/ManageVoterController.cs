using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class ManageVoterController : WebApiController
    {
        public ManageVoterController() { }

        public ManageVoterController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        public List<ManageVoteResponseModel> Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                if (poll.Ballots.Any())
                {
                    List<ManageVoteResponseModel> response = poll
                        .Ballots
                        .Where(b => b.Votes.Any())
                        .Select(CreateManageVoteResponse)
                        .ToList();

                    return response;
                }

                return new List<ManageVoteResponseModel>(0);
            }
        }

        private static ManageVoteResponseModel CreateManageVoteResponse(Ballot ballot)
        {
            var model = new ManageVoteResponseModel
            {
                BallotManageGuid = ballot.ManageGuid,
                Votes = new List<VoteResponse>()
            };

            if (ballot.VoterName != null)
            {
                model.VoterName = ballot.VoterName;
            }

            foreach (Vote vote in ballot.Votes)
            {
                var voteResponse = new VoteResponse
                {
                    ChoiceNumber = vote.Choice.PollChoiceNumber,
                    Value = vote.VoteValue,
                    ChoiceName = vote.Choice.Name
                };
                model.Votes.Add(voteResponse);
            }

            return model;
        }

        [HttpDelete]
        public void Delete(Guid manageId, DeleteVotersRequestModel request)
        {
            if (request == null)
            {
                ThrowError(HttpStatusCode.BadRequest);
            }

            if (!request.BallotDeleteRequests.Any())
            {
                ThrowError(HttpStatusCode.BadRequest);
            }

            if (request.BallotDeleteRequests.Any(b => !b.VoteDeleteRequests.Any()))
            {
                ThrowError(HttpStatusCode.BadRequest);
            }

            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                List<Guid> requestBallotGuids = request
                    .BallotDeleteRequests
                    .Select(b => b.BallotManageGuid)
                    .ToList();

                List<Guid> pollBallotGuids = poll
                    .Ballots
                    .Select(b => b.ManageGuid)
                    .ToList();

                if (requestBallotGuids.Except(pollBallotGuids).Any())
                {
                    ThrowError(HttpStatusCode.NotFound, String.Format("Ballots requested for delete do not all belong to poll {0}", manageId));
                }

                List<Ballot> ballots = poll.Ballots.ToList();

                foreach (DeleteBallotRequestModel ballotRequest in request.BallotDeleteRequests)
                {
                    Ballot ballot = ballots.Single(b => b.ManageGuid == ballotRequest.BallotManageGuid);

                    foreach (DeleteVoteRequestModel voteRequest in ballotRequest.VoteDeleteRequests)
                    {
                        Vote vote = ballot.Votes.ToList().SingleOrDefault(v => v.Choice.PollChoiceNumber == voteRequest.ChoiceNumber);

                        if (vote == null)
                        {
                            ThrowError(HttpStatusCode.NotFound, String.Format("Ballot {0} does not contain an option {1}", ballotRequest.BallotManageGuid, voteRequest.ChoiceNumber));
                        }

                        _metricHandler.HandleVoteDeletedEvent(vote, poll.UUID);
                        ballot.Votes.Remove(vote);
                        context.Votes.Remove(vote);
                    }

                    if (!ballot.Votes.Any())
                    {
                        poll.Ballots.Remove(ballot);
                        context.Ballots.Remove(ballot);
                    }
                }

                poll.LastUpdatedUtc = DateTime.UtcNow;

                context.SaveChanges();
            }
        }
    }
}