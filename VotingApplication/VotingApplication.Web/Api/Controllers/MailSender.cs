using SendGrid;
using SendGrid.SmtpApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using VotingApplication.Web.Api.Logging;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class MailSender : IMailSender
    {
        private NetworkCredential credentials;

        public MailSender()
        {
            string hostEmail = WebConfigurationManager.AppSettings["HostLogin"];
            string hostPassword = WebConfigurationManager.AppSettings["HostPassword"];

            if (hostEmail == null || hostPassword == null)
            {
                return;
            }

            credentials = new NetworkCredential(hostEmail, hostPassword);
        }

        public void SendMail(string to, string subject, string message)
        {
            if (to == null || to.Length == 0)
            {
                return;
            }

            try
            {
                string hostEmail = WebConfigurationManager.AppSettings["HostEmail"];

                SendGridMessage mail = new SendGridMessage();
                mail.From = new MailAddress(hostEmail, "Voting App");
                mail.AddTo(to);
                mail.Subject = subject;
                mail.Text = message;

                var transportWeb = new SendGrid.Web(credentials);
                transportWeb.Deliver(mail);
            }
            catch (Exception exception)
            {
                ILogger logger = LoggerFactory.GetLogger();
                logger.Log(exception);
            }
        }
    }
}
