using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Controllers;
using System.Collections.Generic;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageController : WebApiController
    {
        public ManageController() : base() { }
        public ManageController(IContextFactory contextFactory) : base(contextFactory) { }

        private ManagePollRequestResponseModel PollToModel(Poll poll)
        {
            return new ManagePollRequestResponseModel
            {
                UUID = poll.UUID,
                Options = poll.Options,
                Voters = poll.Tokens,
                VotingStrategy = poll.PollType.ToString(),
                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,
                InviteOnly = poll.InviteOnly,
                Name = poll.Name,
                NamedVoting = poll.NamedVoting,
                RequireAuth = poll.RequireAuth,
                Expires = poll.Expires,
                ExpiryDate = poll.ExpiryDate,
                OptionAdding = poll.OptionAdding
            };
        }

        #region GET

        public ManagePollRequestResponseModel Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls
                    .Where(p => p.ManageId == manageId)
                    .Include(p => p.Options)
                    .Include(p => p.Tokens)
                    .FirstOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                return PollToModel(poll);
            }
        }

        #endregion

        #region Put

        public void Put(Guid manageId, ManagePollUpdateRequest updateRequest)
        {

            #region Input Validation

            if (updateRequest == null)
            {
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            if (updateRequest.Expires && (updateRequest.ExpiryDate == null || updateRequest.ExpiryDate < DateTime.Now))
            {
                ModelState.AddModelError("ExpiryDate", "Invalid or unspecified ExpiryDate");
            }

            if(updateRequest.Options != null)
            {
                foreach(Option option in updateRequest.Options)
                {
                    if(option.Name == null || option.Name == String.Empty)
                    {
                        ModelState.AddModelError("Option.Name", "Invalid or unspecified Option Name");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                this.ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            using (var context = _contextFactory.CreateContext())
            {
                Poll existingPoll = context.Polls
                                           .Where(p => p.ManageId == manageId)
                                           .Include(p => p.Options)
                                           .Include(p => p.Tokens)
                                           .SingleOrDefault();

                if (existingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                existingPoll.NamedVoting = updateRequest.NamedVoting;
                existingPoll.Expires = updateRequest.Expires;
                existingPoll.ExpiryDate = updateRequest.ExpiryDate;
                existingPoll.InviteOnly = updateRequest.InviteOnly;
                existingPoll.MaxPerVote = updateRequest.MaxPerVote;
                existingPoll.MaxPoints = updateRequest.MaxPoints;
                existingPoll.Name = updateRequest.Name;
                existingPoll.OptionAdding = updateRequest.OptionAdding;
                existingPoll.RequireAuth = updateRequest.RequireAuth;

                List<Option> newOptions = new List<Option>();
                List<Option> oldOptions = new List<Option>();
                List<Vote> oldVotes = new List<Vote>();

                if (updateRequest.Options != null && updateRequest.Options.Count > 0)
                {
                    // Match up duplicates and clear out votes of options that are deleted
                    foreach (Option option in existingPoll.Options)
                    {
                        Option duplicateRequestOption = updateRequest.Options.Find(o => o.Id == option.Id);

                        if (duplicateRequestOption != null)
                        {
                            option.Name = duplicateRequestOption.Name;
                            option.Description = duplicateRequestOption.Description;

                            newOptions.Add(option);
                            updateRequest.Options.Remove(duplicateRequestOption);
                        }
                        else
                        {
                            oldOptions.Add(option);
                            oldVotes.AddRange(context.Votes.Where(v => v.OptionId == option.Id).ToList());
                        }
                    }

                    newOptions.AddRange(updateRequest.Options);
                }

                List<Token> redundantTokens = existingPoll.Tokens.ToList<Token>();

                foreach (Token voter in updateRequest.Voters)
                {
                    if (voter.TokenGuid == Guid.Empty)
                    {
                        voter.TokenGuid = Guid.NewGuid();
                        existingPoll.Tokens.Add(voter);
                        // TODO: Send email
                    }
                    else
                    {
                        // Don't mark token as redundant if still in use
                        Token token = redundantTokens.Find(t => t.TokenGuid == voter.TokenGuid);
                        redundantTokens.Remove(token);
                    }
                }

                // Clean up tokens which have been removed
                foreach (Token token in redundantTokens)
                {
                    context.Tokens.Remove(token);
                    existingPoll.Tokens.Remove(token);
                }

                existingPoll.Options = newOptions;
                existingPoll.LastUpdated = DateTime.Now;

                foreach (Option oldOption in oldOptions)
                {
                    context.Options.Remove(oldOption);
                }

                foreach (Vote oldVote in oldVotes)
                {
                    context.Votes.Remove(oldVote);
                }

                // Need code to handle poll type changed when enabaled.

                context.SaveChanges();
            }
        }

        #endregion
    }
}