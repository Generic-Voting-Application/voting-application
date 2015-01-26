using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class TokenPollVoteController : WebApiController
    {
        public TokenPollVoteController() : base() { }
        public TokenPollVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        private Vote ModelToVote(VoteRequestModel voteRequest, Guid tokenGuid, Option option, Poll poll)
        {
            return new Vote
            {
                Option = option,
                Poll = poll,
                PollId = poll.UUID,
                Token = new Token { TokenGuid = tokenGuid },
                VoteValue = voteRequest.VoteValue,
                VoterName = voteRequest.VoterName
            };

        }

        private VoteRequestResponseModel VoteToModel(Vote vote, Poll poll)
        {
            VoteRequestResponseModel model = new VoteRequestResponseModel();

            if (vote.Option != null)
            {
                model.OptionId = vote.Option.Id;
                model.OptionName = vote.Option.Name;
                model.VoterName = vote.VoterName;
            }

            model.VoteValue = vote.VoteValue;

            return model;
        }


        #region GET

        public List<VoteRequestResponseModel> Get(Guid tokenGuid, Guid pollId)
        {

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.UUID == pollId).FirstOrDefault();
                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
                }

                List<Vote> votes = context.Votes.Where(v => v.Token.TokenGuid == tokenGuid && v.PollId == pollId).Include(v => v.Option).ToList();

                return votes.Select(v => VoteToModel(v, poll)).ToList();
            }

        }

        #endregion

        #region PUT

        public void Put(Guid tokenGuid, Guid pollId, List<VoteRequestModel> voteRequests)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (voteRequests == null)
                {
                    this.ThrowError(HttpStatusCode.BadRequest);
                }

                if (!ModelState.IsValid)
                {
                    this.ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                Poll poll = context.Polls.Where(p => p.UUID == pollId).Include(p => p.Tokens).Include(p => p.Options).SingleOrDefault();
                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, String.Format("Poll {0} not found", pollId));
                }

                if (poll.Expires && poll.ExpiryDate < DateTime.Now)
                {
                    this.ThrowError(HttpStatusCode.Forbidden, String.Format("Poll {0} has expired", pollId));
                }

                if (!poll.Tokens.Any(t => t.TokenGuid == tokenGuid))
                {
                    this.ThrowError(HttpStatusCode.Forbidden, String.Format("Token {0} not valid for this poll", tokenGuid));
                }

                foreach (VoteRequestModel voteRequest in voteRequests)
                {
                    if (!context.Options.Any(o => o.Id == voteRequest.OptionId))
                    {
                        ModelState.AddModelError("OptionId", String.Format("Option {0} not found", voteRequest.OptionId));
                    }

                    if (!poll.Options.Any(o => o.Id == voteRequest.OptionId))
                    {
                        ModelState.AddModelError("OptionId", "Option choice not valid for this poll");
                    }

                    if (poll.VotingStrategy == "Points" && (voteRequest.VoteValue > poll.MaxPerVote || voteRequest.VoteValue > poll.MaxPoints))
                    {
                        ModelState.AddModelError("VoteValue", String.Format("Invalid vote value: {0}", voteRequest.VoteValue));
                    }
                }

                if (!ModelState.IsValid)
                {
                    this.ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                List<Vote> existingVotes = context.Votes.Where(v => v.Token.TokenGuid == tokenGuid && v.PollId == pollId).ToList<Vote>();

                foreach (Vote contextVote in existingVotes)
                {
                    context.Votes.Remove(contextVote);
                }

                // For some reason, we don't have an addrange function on Entity Framework
                foreach (VoteRequestModel voteRequest in voteRequests)
                {
                    context.Votes.Add(ModelToVote(voteRequest,
                                                  tokenGuid,
                                                  context.Options.Single(o => o.Id == voteRequest.OptionId),
                                                  context.Polls.Single(p => p.UUID == pollId)));
                }

                poll.LastUpdated = DateTime.Now;

                context.SaveChanges();
            }

            return;
        }

        #endregion
    }
}