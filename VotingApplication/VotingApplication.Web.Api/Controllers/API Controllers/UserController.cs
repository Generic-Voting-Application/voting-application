using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers
{
    public class UserController : WebApiController
    {
        public UserController() : base() { }
        public UserController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get
        public override HttpResponseMessage Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, context.Users.ToList<User>());
            }
        }

        public virtual HttpResponseMessage Get(long id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                User userForId = context.Users.Where(u => u.Id == id).FirstOrDefault();
                if (userForId == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} not found", id));
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, userForId);
                }
            }
        }
        #endregion

        #region Put

        public HttpResponseMessage Put(User newUser)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newUser.Name == null || newUser.Name.Equals(""))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Username must not be empty");
                }
                else if (new Regex(@"[^\w| ]").IsMatch(newUser.Name))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Username must be alpha-numeric");
                }

                Poll matchingPoll = context.Polls.Where(p => p.UUID == newUser.PollId).Include(p => p.Tokens).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User missing a poll");
                }

                // Search for Tokens that match what the user has given
                Token matchingToken = null;
                if (newUser.Token != null)
                {
                    User matchingUser = context.Users.Where(u => u.Token != null
                        && u.Token.TokenGuid == newUser.Token.TokenGuid
                        && u.PollId == matchingPoll.UUID).FirstOrDefault();
                    if (matchingUser != null)
                    {
                        matchingToken = matchingUser.Token;
                        matchingToken.UserId = matchingUser.Id;
                    }
                }

                // If no matching token is found, check if the poll is closed
                if (matchingToken == null)
                {
                    if (matchingPoll.InviteOnly)
                    {
                        // If so, check that the token supplied (if any) is a valid token for the poll
                        if (newUser.Token == null || !matchingPoll.Tokens.Any(t => t.TokenGuid == newUser.Token.TokenGuid))
                        {
                            // If not, throw an error
                            return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User missing a valid token for this poll");
                        }
                        else
                        {
                            // If there is a token, link it up
                            matchingToken = matchingPoll.Tokens.Find(t => t.TokenGuid == newUser.Token.TokenGuid);
                        }
                    }
                    else if (matchingPoll.UUID != Guid.Empty)
                    {
                        // If the poll is open, create a token for the user so they can use it next time
                        matchingToken = new Token() { TokenGuid = Guid.NewGuid(), PollId = matchingPoll.UUID };
                        matchingPoll.Tokens.Add(matchingToken);
                    }
                }

                User existingUser = null;

                if (newUser.Token != null)
                {
                    existingUser = context.Users.Where(u => u.Token != null && u.Token.TokenGuid == newUser.Token.TokenGuid).FirstOrDefault();
                }

                if (existingUser == null)
                {
                    // Save once to generate user ID
                    newUser.Token = matchingToken;
                    context.Users.Add(newUser);
                    context.SaveChanges();

                    matchingToken.UserId = newUser.Id;
                }
                else
                {
                    existingUser.Name = newUser.Name;
                }

                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingToken);
            }
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        public virtual HttpResponseMessage Post(long id, User newUser)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST on this controller");
        }

        #endregion
    }
}