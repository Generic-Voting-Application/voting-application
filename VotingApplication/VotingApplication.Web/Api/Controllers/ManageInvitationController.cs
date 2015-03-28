using System.Reflection;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Services;
using System.IO;
using System.Threading;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageInvitationController : WebApiController
    {
        private IMailSender _mailSender;

        public ManageInvitationController(IMailSender mailSender)
            : base()
        {
            _mailSender = mailSender;
        }
        public ManageInvitationController(IContextFactory contextFactory, IMailSender mailSender) : base(contextFactory)
        {
            _mailSender = mailSender;
        }

        #region POST

        public void Post(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls
                                        .Where(p => p.ManageId == manageId)
                                        .Include(p => p.Ballots)
                                        .FirstOrDefault();

                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                foreach (Ballot ballot in matchingPoll.Ballots)
                {
                    // Already sent email
                    if (ballot.TokenGuid != null && ballot.TokenGuid != Guid.Empty)
                    {
                        continue;
                    }

                    ballot.TokenGuid = Guid.NewGuid();

                    Thread emailThread = new Thread(new ThreadStart(() => SendInvitation(matchingPoll.UUID, ballot, matchingPoll.Name)));
                    emailThread.Start();
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