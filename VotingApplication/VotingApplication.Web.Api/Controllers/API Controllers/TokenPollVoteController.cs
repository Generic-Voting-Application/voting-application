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

        public virtual HttpResponseMessage Get(Guid tokenGuid, Guid pollId)
        {

            List<Vote> votes;
            Poll poll;
            using (var context = _contextFactory.CreateContext())
            {
                poll = context.Polls.Where(s => s.UUID == pollId).FirstOrDefault();
                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
                }

                votes = context.Votes.Where(v => v.Token.TokenGuid == tokenGuid && v.PollId == pollId).Include(v => v.Option).ToList();
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, votes.Select(v => VoteToModel(v, poll)).ToList());

        }


        public virtual HttpResponseMessage Get(Guid tokenGuid, Guid pollId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by id on this controller");
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(Guid tokenGuid, Guid pollId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(Guid tokenGuid, Guid pollId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(Guid tokenGuid, Guid pollId, List<VoteRequestModel> voteRequests)
        {
            #region Input Validation

            if (voteRequests == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(p => p.UUID == pollId).Include(p => p.Tokens).Include(p => p.Options).SingleOrDefault();
                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Poll {0} not found", pollId));
                }

                if (poll.Expires && poll.ExpiryDate < DateTime.Now)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, String.Format("Poll {0} has expired", pollId));
                }

                if (!poll.Tokens.Any(t => t.TokenGuid == tokenGuid))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, String.Format("Token {0} not valid for this poll", tokenGuid));
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
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }

            #endregion

            #region DB Object Creation

            using (var context = _contextFactory.CreateContext())
            {
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

                context.SaveChanges();
            }

            #endregion

            #region Response

            return this.Request.CreateResponse(HttpStatusCode.OK);

            #endregion
        }

        #endregion

        #region DELETE

        public virtual HttpResponseMessage Delete(Guid tokenGuid, Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(Guid tokenGuid, Guid pollId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by id on this controller");
        }

        #endregion

    }
}