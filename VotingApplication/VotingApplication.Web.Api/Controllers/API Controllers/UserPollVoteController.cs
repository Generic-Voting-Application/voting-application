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
    public class UserPollVoteController : WebApiController
    {
        public UserPollVoteController() : base() { }
        public UserPollVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        private VoteRequestResponseModel VoteToModel(Vote vote, Poll poll)
        {
            VoteRequestResponseModel model = new VoteRequestResponseModel();

            if (vote.Option != null)
            {
                model.OptionId = vote.Option.Id;
                model.OptionName = vote.Option.Name;
            }

            if (vote.User != null)
            {
                model.VoterName = poll.AnonymousVoting ? "Anonymous User" : vote.User.Name;
                model.UserId = vote.User.Id;
            }

            model.VoteValue = vote.PollValue;

            return model;
        }

        private Vote ModelToVote(VoteRequestModel voteRequest, Option option, Poll poll, User user)
        {
           return  new Vote
            {
                Option = option,//context.Options.Single(o => o.Id == voteRequest.OptionId),
                Poll = poll,//context.Polls.Single(p => p.UUID == pollId),
                PollId = poll.UUID,
                Token = new Token { PollId = poll.UUID, UserId = user.Id, TokenGuid = voteRequest.TokenGuid },
                User = user,//context.Users.Single(u => u.Id == userId),
                PollValue = voteRequest.VoteValue
            };
            
        }

        public virtual HttpResponseMessage Get(long userId, Guid pollId)
        {

            #region DBGet / Validation

            List<Vote> votes;
            Poll poll;
            using (var context = _contextFactory.CreateContext())
            {
                User user = context.Users.Where(u => u.Id == userId).FirstOrDefault();
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} not found", userId));
                }

                poll = context.Polls.Where(s => s.UUID == pollId).FirstOrDefault();
                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", pollId));
                }

                votes = context.Votes.Where(v => v.UserId == userId && v.PollId == pollId).Include(v => v.Option).ToList();
            }

            #endregion

            return this.Request.CreateResponse(HttpStatusCode.OK, votes.Select(v => VoteToModel(v, poll)).ToList());
        }

        #endregion

        #region POST

        public virtual HttpResponseMessage Post(long userId, Guid pollId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long userId, Guid pollId, long voteId, Vote vote)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by id on this controller");
        }

        #endregion

        #region PUT

        public virtual HttpResponseMessage Put(long userId, Guid pollId, List<VoteRequestModel> voteRequests)
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
                if (!context.Users.Any(u => u.Id == userId))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("User {0} not found", userId));
                }

                Poll poll = context.Polls.Where(p => p.UUID == pollId).Include(p => p.Tokens).Include(p => p.Options).SingleOrDefault();
                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Poll {0} not found", pollId));
                }

                if (poll.Expires && poll.ExpiryDate < DateTime.Now)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, String.Format("Poll {0} has expired", pollId));
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

                    if (!poll.Tokens.Any(t => t.TokenGuid == voteRequest.TokenGuid))
                    {
                        ModelState.AddModelError("TokenGuid", String.Format("Token {0} not valid for this poll", voteRequest.TokenGuid));
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

                // TODO: This needs to be changed
                List<Vote> existingVotes = context.Votes.Where(v => v.UserId == userId && v.PollId == pollId).ToList<Vote>();

                foreach (Vote contextVote in existingVotes)
                {
                    context.Votes.Remove(contextVote);
                }

                // For some reason, we don't have an addrange function on Entity Framework
                foreach (VoteRequestModel voteRequest in voteRequests)
                {
                    context.Votes.Add(ModelToVote(voteRequest, context.Options.Single(o => o.Id == voteRequest.OptionId), 
                                                               context.Polls.Single(p => p.UUID == pollId),
                                                               context.Users.Single(u => u.Id == userId)));
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

        public virtual HttpResponseMessage Delete(long userId, Guid pollId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public virtual HttpResponseMessage Delete(long userId, Guid pollId, long voteId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by id on this controller");
        }

        #endregion

    }
}