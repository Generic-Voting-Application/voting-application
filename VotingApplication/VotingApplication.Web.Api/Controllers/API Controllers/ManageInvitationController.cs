using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Configuration;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageInvitationController : WebApiController
    {
        private IMailSender _mailSender;

        public ManageInvitationController() : base()
        {
            _mailSender = new MailSender();
        }
        public ManageInvitationController(IContextFactory contextFactory, IMailSender mailSender) : base(contextFactory)
        {
            _mailSender = mailSender;
        }

        #region POST

        public void Post(Guid manageId, List<string> invitationEmails)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(p => p.ManageId == manageId).Include(p => p.Tokens).FirstOrDefault();
                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                SendVoteEmails(matchingPoll, invitationEmails);
                context.SaveChanges();

                return;
            }
        }

        private void SendVoteEmails(Poll poll, List<string> invitationEmails)
        {
            String hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (hostUri == String.Empty)
            {
                return;
            }

            foreach (string targetEmailAddress in invitationEmails)
            {
                if (String.IsNullOrEmpty(targetEmailAddress))
                {
                    continue;
                }

                //Remove leading/trailing whitespace
                var emailAddress = targetEmailAddress.Trim();
                //Check email is correctly formed to avoid costly sending of invalid emails, potentially dropping our SendGrid reputation
                if (!Regex.IsMatch(emailAddress, @"^[\w._%+-]+@\w+(\.\w+)+$", RegexOptions.IgnoreCase))
                {
                    continue;
                }


                string tokenString = "";
                if (poll.InviteOnly)
                {
                    Token token = new Token() { TokenGuid = Guid.NewGuid() };
                    tokenString = "/" + token.TokenGuid;
                    poll.Tokens.Add(token);
                }

                string message = String.Join("\n\n", new List<string>()
                {"You've been invited by " + poll.Creator + " to vote on " + poll.Name,
                 "Have your say at: " + hostUri + "/Poll/Index/" + poll.UUID + tokenString});

                Thread newThread = new Thread(new ThreadStart(() => _mailSender.SendMail(emailAddress, "Have your say!", message)));
                newThread.Start();
            }
        }

        #endregion

    }
}