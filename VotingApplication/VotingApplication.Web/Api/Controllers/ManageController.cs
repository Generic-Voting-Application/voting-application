using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Configuration;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageController : WebApiController
    {

        private IMailSender _mailSender;

        public ManageController() : base() { }

        public ManageController(IContextFactory contextFactory, IMailSender mailSender)
            : base(contextFactory)
        {
            _mailSender = mailSender;
        }

        private TokenRequestModel TokenToModel(Ballot ballot)
        {
            return new TokenRequestModel
            {
                Email = ballot.Email,
                TokenGuid = ballot.TokenGuid,
                Name = ballot.VoterName
            };
        }

        private ManagePollRequestResponseModel PollToModel(Poll poll)
        {
            List<TokenRequestModel> Voters = poll.Ballots.ConvertAll<TokenRequestModel>(TokenToModel);

            return new ManagePollRequestResponseModel
            {
                UUID = poll.UUID,
                Options = poll.Options,
                Voters = Voters,
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

        #region GET

        public ManagePollRequestResponseModel Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls
                    .Where(p => p.ManageId == manageId)
                    .Include(p => p.Options)
                    .Include(p => p.Ballots)
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
                this.ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls
                                           .Where(p => p.ManageId == manageId)
                                           .Include(p => p.Options)
                                           .Include(p => p.Ballots)
                                           .SingleOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll for manage id {0} not found", manageId));
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

                List<Ballot> redundantTokens = poll.Ballots.ToList<Ballot>();

                foreach (TokenRequestModel voter in updateRequest.Voters)
                {
                    if (voter.TokenGuid == null)
                    {
                        Ballot newBallot = new Ballot { Email = voter.Email, TokenGuid = Guid.NewGuid() };
                        poll.Ballots.Add(newBallot);
                        SendInvitation(poll.UUID, newBallot, poll.Name);
                    }
                    else
                    {
                        // Don't mark token as redundant if still in use
                        Ballot ballot = redundantTokens.Find(t => t.TokenGuid == voter.TokenGuid);
                        redundantTokens.Remove(ballot);
                    }
                }

                // Clean up tokens which have been removed
                foreach (Ballot token in redundantTokens)
                {
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

        #region Email Sending

        private string HtmlFromFile(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void SendInvitation(Guid UUID, Ballot ballot, string pollQuestion)
        {
            if (string.IsNullOrEmpty(ballot.Email))
            {
                return;
            }

            string hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (hostUri == String.Empty)
            {
                return;
            }

            string link = hostUri + "/Poll/#/Vote/" + UUID + "/" + ballot.TokenGuid;

            string htmlMessage = HtmlFromFile("VotingApplication.Web.Api.Resources.EmailTemplate.html");
            htmlMessage = htmlMessage.Replace("__VOTEURI__", link);
            htmlMessage = htmlMessage.Replace("__HOSTURI__", hostUri);
            htmlMessage = htmlMessage.Replace("__POLLQUESTION__", pollQuestion);

            _mailSender.SendMail(ballot.Email, "Have your say", htmlMessage);
        }

        #endregion

        #endregion
    }
}