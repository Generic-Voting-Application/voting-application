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
    public class ManageController : WebApiController
    {

        public ManageController() : base() { }

        public ManageController(IContextFactory contextFactory) : base(contextFactory) { }

        [HttpGet]
        public ManagePollRequestResponseModel Get(Guid manageId)
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls
                    .Where(p => p.ManageId == manageId)
                    .Include(p => p.Options)
                    .Include(p => p.Ballots)
                    .Include(p => p.Ballots.Select(b => b.Votes))
                    .FirstOrDefault();

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                return CreateResponseModelFromPoll(poll);
            }
        }

        private ManagePollRequestResponseModel CreateResponseModelFromPoll(Poll poll)
        {
            List<ManagePollBallotRequestModel> Voters = poll
                .Ballots
                .Where(b => b.Votes.Any())
                .Select(CreateManagePollRequestResponseModel)
                .ToList();

            List<Ballot> Invitees = poll
                .Ballots
                .Where(b => !String.IsNullOrWhiteSpace(b.Email))
                .ToList<Ballot>();

            return new ManagePollRequestResponseModel
            {
                UUID = poll.UUID,
                Options = poll.Options,
                Voters = Voters,
                InviteeCount = Invitees.Count,
                VotingStrategy = poll.PollType.ToString(),
                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,
                InviteOnly = poll.InviteOnly,
                Name = poll.Name,
                NamedVoting = poll.NamedVoting,
                ExpiryDate = poll.ExpiryDate,
                OptionAdding = poll.OptionAdding
            };
        }

        private ManagePollBallotRequestModel CreateManagePollRequestResponseModel(Ballot ballot)
        {
            return new ManagePollBallotRequestModel
            {
                Email = ballot.Email,
                EmailSent = (ballot.TokenGuid != null && ballot.TokenGuid != Guid.Empty),
                ManageToken = ballot.ManageGuid
            };
        }

        #region Put

        public void Put(Guid manageId, ManagePollUpdateRequest updateRequest)
        {

            #region Input Validation

            if (updateRequest == null)
            {
                ThrowError(HttpStatusCode.BadRequest);
            }

            if (updateRequest.ExpiryDate.HasValue && updateRequest.ExpiryDate < DateTime.Now)
            {
                ModelState.AddModelError("ExpiryDate", "Invalid ExpiryDate");
            }

            if (updateRequest.Options != null)
            {
                foreach (Option option in updateRequest.Options)
                {
                    if (option.Name == null || option.Name == String.Empty)
                    {
                        ModelState.AddModelError("Option.Name", "Invalid or unspecified Option Name");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls
                                           .Where(p => p.ManageId == manageId)
                                           .Include(p => p.Options)
                                           .Include(p => p.Ballots)
                                           .Include(p => p.Ballots.Select(b => b.Votes))
                                           .SingleOrDefault();

                if (poll == null)
                {
                    ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
                }

                poll.NamedVoting = updateRequest.NamedVoting;
                poll.ExpiryDate = updateRequest.ExpiryDate;
                poll.InviteOnly = updateRequest.InviteOnly;
                poll.MaxPerVote = updateRequest.MaxPerVote;
                poll.MaxPoints = updateRequest.MaxPoints;
                poll.Name = updateRequest.Name;
                poll.OptionAdding = updateRequest.OptionAdding;

                List<Option> newOptions = new List<Option>();
                List<Option> oldOptions = new List<Option>();
                List<Vote> removedVotes = new List<Vote>();

                if (updateRequest.Options != null && updateRequest.Options.Count > 0)
                {
                    // Match up duplicates and clear out votes of options that are deleted
                    foreach (Option option in poll.Options)
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
                            IEnumerable<Vote> votes = context
                                .Votes
                                .Include(v => v.Option)
                                .Where(v => v.Option.Id == option.Id)
                                .ToList();

                            removedVotes.AddRange(votes);
                        }
                    }

                    newOptions.AddRange(updateRequest.Options);
                }

                poll.Options = newOptions;
                poll.LastUpdated = DateTime.Now;

                if (updateRequest.VotingStrategy.ToLower() != poll.PollType.ToString().ToLower())
                {
                    removedVotes.AddRange(
                        context
                        .Votes
                        .Include(v => v.Poll)
                        .Where(v => v.Poll.UUID == poll.UUID)
                        .ToList()
                        );

                    poll.PollType = (PollType)Enum.Parse(typeof(PollType), updateRequest.VotingStrategy, true);
                }

                List<Ballot> redundantTokens = poll.Ballots.Where(b => b.Email != null).ToList<Ballot>();

                foreach (ManagePollBallotRequestModel voter in updateRequest.Voters)
                {
                    Ballot ballot = redundantTokens.Find(t => t.ManageGuid == voter.ManageToken);

                    // Don't mark token as redundant if still in use
                    if (ballot != null)
                    {
                        redundantTokens.Remove(ballot);
                    }
                    else
                    {
                        ballot = new Ballot { Email = voter.Email, ManageGuid = Guid.NewGuid() };
                        poll.Ballots.Add(ballot);
                    }
                }

                // Clean up tokens which have been removed
                foreach (Ballot token in redundantTokens)
                {
                    foreach (Vote redundantVote in token.Votes.ToList())
                    {
                        removedVotes.Add(redundantVote);
                        token.Votes.Remove(redundantVote);
                    }

                    context.Ballots.Remove(token);
                    poll.Ballots.Remove(token);
                }

                poll.Options = newOptions;
                poll.LastUpdated = DateTime.Now;

                foreach (Option oldOption in oldOptions)
                {
                    context.Options.Remove(oldOption);
                }

                foreach (Vote oldVote in removedVotes)
                {
                    context.Votes.Remove(oldVote);
                }

                context.SaveChanges();
            }
        }

        #endregion
    }
}