using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
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

        #region GET

        public HttpResponseMessage Get(Guid manageId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET on this controller");
        }

        public HttpResponseMessage Get(Guid manageId, long invitationId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET by Id on this controller");
        }

        #endregion

        #region POST

        public HttpResponseMessage Post(Guid manageId, List<string> invitationEmails)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(p => p.ManageID == manageId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                SendVoteEmails(matchingPoll, invitationEmails);

                return this.Request.CreateResponse(HttpStatusCode.OK);
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
                    Token token = new Token() { PollId = poll.UUID, TokenGuid = Guid.NewGuid() };
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

        public HttpResponseMessage Post(Guid manageId, long invitationId, string invitation)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use POST by Id on this controller");
        }

        #endregion

        #region PUT

        public HttpResponseMessage Put(Guid manageId, List<string> invitation)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        public HttpResponseMessage Put(Guid manageId, long invitationId, string invitation)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT by Id on this controller");
        }

        #endregion

        #region DELETE

        public HttpResponseMessage Delete(Guid manageId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE on this controller");
        }

        public HttpResponseMessage Delete(Guid manageId, long invitationId)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use DELETE by Id on this controller");
        }

        #endregion

    }
}