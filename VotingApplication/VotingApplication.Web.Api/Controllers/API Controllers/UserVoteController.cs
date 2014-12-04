using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserVoteController : WebApiController
    {
        public UserVoteController() : base() { }
        public UserVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        #region GET

        public virtual HttpResponseMessage Get(long userId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userExists = (context.Users.Where(u => u.Id == userId).Count() == 1);
                if (userExists)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, context.Votes.Where(v => v.UserId == userId).Include(v => v.Option).Include(v => v.User).ToList<Vote>());
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} not found", userId));
                }
            }
        }

        public HttpResponseMessage Get(long userId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var userVoteResponse = Get(userId);
                List<Vote> userVotes = ((ObjectContent)userVoteResponse.Content).Value as List<Vote>;
                if (userVotes == null)
                {
                    return userVoteResponse;
                }

                Vote matchingVote = userVotes.Where(v => v.Id == voteId).FirstOrDefault();
                if (matchingVote != null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, matchingVote);
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Vote {0} not found", voteId));
                }
            }

        }

        #endregion

        #region PUT

        public HttpResponseMessage Put(long userId, List<Vote> votes)
        {
            using (var context = _contextFactory.CreateContext())
            {
                List<long> voteIds = new List<long>();

                foreach (Vote vote in votes)
                {
                    if (vote.OptionId == 0)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Vote does not have an option");
                    }

                    if (vote.PollId == Guid.Empty)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Vote does not have a poll");
                    }

                    IEnumerable<User> users = context.Users.Where(u => u.Id == userId);
                    if (users.Count() == 0)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("User {0} does not exist", userId));
                    }

                    IEnumerable<Option> options = context.Options.Where(o => o.Id == vote.OptionId);
                    if (options.Count() == 0)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Option {0} does not exist", vote.OptionId));
                    }

                    IEnumerable<Poll> polls = context.Polls.Where(p => p.UUID == vote.PollId).Include(p => p.Tokens);
                    if (polls.Count() == 0)
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Poll {0} does not exist", vote.PollId));
                    }

                    Poll poll = polls.FirstOrDefault();
                    Boolean isTokenPoll = poll.InviteOnly;

                    if (isTokenPoll)
                    {
                        // Validate tokens if required
                        if (vote.Token == null || vote.Token.TokenGuid == Guid.Empty)
                        {
                            return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Token required for this poll");
                        }
                        else if (poll.Tokens == null || !poll.Tokens.Any(t => t.TokenGuid == vote.Token.TokenGuid))
                        {
                            return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, String.Format("Invalid token: {0}", vote.Token));
                        }
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

                    List<Vote> contextVotes;

                    if(isTokenPoll) 
                    {
                        contextVotes = context.Votes.Where(v => v.Token != null && v.Token.TokenGuid == vote.Token.TokenGuid && v.PollId == vote.PollId).ToList<Vote>();
                    }
                    else
                    {
                        contextVotes = context.Votes.Where(v => v.UserId == userId && v.PollId == vote.PollId).ToList<Vote>();
                    }
                    
                    foreach (Vote contextVote in contextVotes)
                    {
                        context.Votes.Remove(contextVote);
                    }

                    vote.UserId = userId;
                }

                foreach (Vote vote in votes)
                {
                    context.Votes.Add(vote);
                    context.SaveChanges();
                    voteIds.Add(vote.Id);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, voteIds);
            }
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long id, Vote newUser)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion
    }
}