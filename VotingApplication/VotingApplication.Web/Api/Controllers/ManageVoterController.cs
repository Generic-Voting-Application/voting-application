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
    public class ManageVoterController : WebApiController
    {
        public ManageVoterController()
        {
        }

        public ManageVoterController(IContextFactory contextFactory)
            : base(contextFactory)
        {
        }

        [HttpGet]
        public List<ManageVoteResponseModel> Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context
                    .Polls
                    .Include(p => p.Ballots)
                    .Include(p => p.Ballots.Select(b => b.Votes))
                    .Include(p => p.Ballots.Select(b => b.Votes.Select(v => v.Option)))
                    .SingleOrDefault(s => s.ManageId == manageId);

                if (poll == null)
                {
                    string errorMessage = String.Format("Poll {0} not found", manageId);
                    this.ThrowError(HttpStatusCode.NotFound, errorMessage);
                }

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
                VoterName = ballot.VoterName,
                Votes = new List<VoteResponse>()
            };

            foreach (Vote vote in ballot.Votes)
            {
                var voteResponse = new VoteResponse
                {
                    OptionNumber = vote.Option.PollOptionNumber,
                    Value = vote.VoteValue,
                    OptionName = vote.Option.Name
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
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            if (!request.BallotDeleteRequests.Any())
            {
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            if (request.BallotDeleteRequests.Any(b => !b.VoteDeleteRequests.Any()))
            {
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = context
                    .Polls
                    .Include(p => p.Ballots)
                    .Include(p => p.Ballots.Select(b => b.Votes))
                    .SingleOrDefault(s => s.ManageId == manageId);

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, String.Format("Poll {0} not found", manageId));
                }

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
                    this.ThrowError(HttpStatusCode.NotFound, String.Format("Ballots requested for delete do not all belong to poll {0}", manageId));
                }

                List<Ballot> ballots = poll.Ballots.ToList();

                foreach (DeleteBallotRequestModel ballotRequest in request.BallotDeleteRequests)
                {
                    Ballot ballot = ballots.Single(b => b.ManageGuid == ballotRequest.BallotManageGuid);

                    foreach (DeleteVoteRequestModel voteRequest in ballotRequest.VoteDeleteRequests)
                    {
                        Vote vote = ballot.Votes.ToList().SingleOrDefault(v => v.Option.PollOptionNumber == voteRequest.OptionNumber);

                        if (vote == null)
                        {
                            this.ThrowError(HttpStatusCode.NotFound, String.Format("Ballot {0} does not contain an option {1}", ballotRequest.BallotManageGuid, voteRequest.OptionNumber));
                        }

                        ballot.Votes.Remove(vote);
                        context.Votes.Remove(vote);
                    }

                    if (!ballot.Votes.Any())
                    {
                        poll.Ballots.Remove(ballot);
                        context.Ballots.Remove(ballot);
                    }
                }

                poll.LastUpdated = DateTime.Now;

                context.SaveChanges();
            }
        }
    }
}