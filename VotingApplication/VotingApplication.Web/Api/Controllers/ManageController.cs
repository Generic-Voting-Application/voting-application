using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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

        private TokenRequestModel TokenToModel(Token token)
        {
            return new TokenRequestModel
            {
                Email = token.Email,
                TokenGuid = token.TokenGuid
            };
        }

        private ManagePollRequestResponseModel PollToModel(Poll poll)
        {
            List<TokenRequestModel> Voters = poll.Tokens.ConvertAll<TokenRequestModel>(TokenToModel);

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
                                           .Include(p => p.Tokens)
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

                List<Token> redundantTokens = poll.Tokens.ToList<Token>();

                foreach (TokenRequestModel voter in updateRequest.Voters)
                {
                    if (voter.TokenGuid == null)
                    {
                        Token newToken = new Token { Email = voter.Email, TokenGuid = Guid.NewGuid() };
                        poll.Tokens.Add(newToken);
                        SendInvitation(poll.UUID, newToken, poll.Name);
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
                    poll.Tokens.Remove(token);
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

        private void SendInvitation(Guid UUID, Token token, string pollQuestion)
        {
            if (string.IsNullOrEmpty(token.Email))
            {
                return;
            }

            String hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (hostUri == String.Empty)
            {
                return;
            }

            var link = hostUri + "/Poll/#/Vote/" + UUID + "/" + token.TokenGuid;

            string emailCss = @"
<style type='text/css'>
    button{background-color:#4caf50;border:1px solid #388e3c;padding:5px 10px;color:#fff;border-radius:5px;font-size:12pt;font-weight:700}html{position:absolute;top:0;bottom:0}body{border-color:#C8E6C9;border-style:solid;border-width:0 10px;max-width:400px;font-family:'open sans';text-align:center;margin:0}div{margin:auto;width:95%;background-color:#fff}.left{float:left;margin-left:20px}.right{float:right;margin-right:20px}.clear{clear:both;margin-bottom:30px}span{font-weight:700}hr{width:80%;background-color:#bbb;margin-top:15px;margin-bottom:30px;border-style:none;height:1px}h2{margin-top:5px}.smalltext{font-size:8pt;color:#888}.smalltext a{color:#888}
</style>";

            string message = @"
<div>
    <a class='left' href='" + hostUri + @"'><img alt='Vote On' src='http://votingapp.azurewebsites.net/Images/Logo.png' /></a>
    <p class='clear'></p>
    <div>
        You've been invited to Vote On
        <h2>Where shall we go for lunch?</h2>
        <a href='" + link + @"'><button>Vote Now</button></a>
        <hr />
    </div>
    <p class='smalltext'>
        You have been invited to vote on this question, using the main portal site.
        If you decide not to vote, the poll will still go ahead but you'll miss your chance to express an opinion.
        Some polls are time-limited, so the sooner you respond, the better.
        <br />
        <br />
        Your email will not be used for marketing purposes,
        only for this poll. The 'Vote On' name and brand is an unregistered trademark, and the property of <a href='http://www.scottlogic.com'>Scott Logic Ltd</a>.
    </p>
</div>
<div style='clear: both;'></div>";

            string htmlMessage = "<html><head>" + emailCss + "</head><body>" + message + "</body></html>";
            _mailSender.SendMail(token.Email, "Have your say", htmlMessage);
        }

        #endregion

        #endregion
    }
}