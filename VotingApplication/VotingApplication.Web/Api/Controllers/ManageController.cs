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

        public ManageController(IContextFactory contextFactory, IMailSender mailSender) : base(contextFactory)
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
                            removedVotes.AddRange(context.Votes.Where(v => v.OptionId == option.Id).ToList());
                        }
                    }

                    newOptions.AddRange(updateRequest.Options);
                }

                poll.Options = newOptions;
                poll.LastUpdated = DateTime.Now;

                if (updateRequest.VotingStrategy.ToLower() != poll.PollType.ToString().ToLower())
                {
                    removedVotes.AddRange(context.Votes.Where(v => v.PollId == poll.UUID).ToList());
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
    <a class='left' href='" + hostUri + @"'><img alt='Vote On' src='data:image/png;base64,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' /></a>
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