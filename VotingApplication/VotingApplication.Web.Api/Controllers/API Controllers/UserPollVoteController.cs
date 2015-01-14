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

            #region Response

            List<PollVoteRequestResponseModel> response = new List<PollVoteRequestResponseModel>();

            foreach (Vote vote in votes)
            {
                PollVoteRequestResponseModel responseVote = new PollVoteRequestResponseModel();

                if (vote.Option != null)
                {
                    responseVote.OptionId = vote.Option.Id;
                    responseVote.OptionName = vote.Option.Name;
                }

                if (vote.User != null)
                {
                    responseVote.VoterName = poll.AnonymousVoting ? "Anonymous User" : vote.User.Name;
                    responseVote.UserId = vote.User.Id;
                }

                responseVote.VoteValue = vote.PollValue;

                response.Add(responseVote);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, response);

            #endregion

        }

        /*

        public virtual HttpResponseMessage Get(long userId, Guid pollId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userPollResponse = Get(userId, pollId);
                List<Vote> votesForUserPoll = ((ObjectContent)userPollResponse.Content).Value as List<Vote>;
                if (votesForUserPoll == null)
                {
                    return userPollResponse;
                }

                Vote matchingVote = votesForUserPoll.Where(v => v.Id == voteId).FirstOrDefault();
                if (matchingVote == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Vote {0} not found", voteId));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingVote);
            }
        }

        */

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

                foreach (VoteRequestModel voteRequest in voteRequests)
                {
                    Vote newVote = new Vote();
                    newVote.Option = context.Options.Single(o => o.Id == voteRequest.OptionId);
                    newVote.Poll = context.Polls.Single(p => p.UUID == pollId);
                    newVote.PollId = pollId;
                    newVote.Token = new Token { PollId = pollId, UserId = userId, TokenGuid = voteRequest.TokenGuid };
                    newVote.User = context.Users.Single(u => u.Id == userId);
                    newVote.PollValue = voteRequest.VoteValue;

                    context.Votes.Add(newVote);
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