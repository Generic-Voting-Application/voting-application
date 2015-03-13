using Microsoft.AspNet.Identity;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace VotingApplication.Web.Api.Services
{
    public class SendMailEmailSender : IMailSender
    {
        private NetworkCredential credentials;
        private string hostEmail;

        public SendMailEmailSender(NetworkCredential credentials, string hostEmail)
        {
            this.credentials = credentials;
            this.hostEmail = hostEmail;
        }

        public Task SendMail(string to, string subject, string message)
        {
            SendGridMessage mail = new SendGridMessage();

            mail.From = new MailAddress(this.hostEmail, "Voting App");
            mail.AddTo(to);
            mail.Subject = subject;
            mail.Html = message;

            var transportWeb = new SendGrid.Web(this.credentials);

            return transportWeb.DeliverAsync(mail);
        }
    }
}