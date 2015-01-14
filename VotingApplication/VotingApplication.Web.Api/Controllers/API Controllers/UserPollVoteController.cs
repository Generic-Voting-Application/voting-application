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

        public virtual HttpResponseMessage Put(long userId, Guid pollId, List<Vote> votes)
        {
            using (var context = _contextFactory.CreateContext())
            {
                IEnumerable<User> users = context.Users.Where(u => u.Id == userId).Include(u => u.Token);
                if (users.Count() == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("User {0} not found", userId));
                }

                Poll poll = context.Polls.Where(p => p.UUID == pollId).Include(p => p.Tokens).Include(p => p.Options).FirstOrDefault();
                if (poll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Poll {0} not found", pollId));
                }

                User user = users.FirstOrDefault();

                Guid userTokenId;
                if (user.Token == null)
                {
                    userTokenId = Guid.Empty;
                }
                else
                {
                    userTokenId = user.Token.TokenGuid;
                }

                // Clear out existing votes for this user in this poll
                List<Vote> contextVotes = context.Votes.Where(v => v.Token != null && v.Token.TokenGuid == userTokenId && v.PollId == pollId).ToList<Vote>();

                foreach (Vote contextVote in contextVotes)
                {
                    context.Votes.Remove(contextVote);
                }

                List<long> voteIds = new List<long>();

                foreach (Vote vote in votes)
                {
                    if (vote.OptionId == 0)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Vote must specify an option");
                    }

                    IEnumerable<Option> options = context.Options.Where(o => o.Id == vote.OptionId);
                    if (options.Count() == 0)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Option {0} not found", vote.OptionId));
                    }

                    if (poll.Expires && poll.ExpiryDate < DateTime.Now)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, String.Format("Poll {0} has expired", pollId));
                    }

                    // Check that the option is valid for the poll
                    Option option = options.FirstOrDefault();
                    if (poll.Options == null || poll.Options.Count == 0 || !poll.Options.Contains(option))
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, String.Format("Option choice not valid for poll {0}", pollId));
                    }

                    // Validate tokens if required
                    if (vote.Token == null || vote.Token.TokenGuid == Guid.Empty)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, String.Format("A valid token is required for poll {0}", pollId));
                    }
                    else if (poll.Tokens == null || !poll.Tokens.Any(t => t.TokenGuid == vote.Token.TokenGuid))
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, String.Format("Invalid token: {0}", vote.Token.TokenGuid));
                    }

                    // Validate poll value
                    if (vote.PollValue <= 0)
                    {
                        vote.PollValue = 1;
                    }

                    if (poll.VotingStrategy == "Points" && (vote.PollValue > poll.MaxPerVote || vote.PollValue > poll.MaxPoints))
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, String.Format("Invalid vote value: {0}", vote.PollValue));
                    }

                    vote.UserId = userId;
                }

                foreach (Vote vote in votes)
                {
                    vote.PollId = pollId;
                    context.Votes.Add(vote);
                    context.SaveChanges();
                    voteIds.Add(vote.Id);
                }

                poll.LastUpdated = DateTime.UtcNow;

                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, voteIds);
            }

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