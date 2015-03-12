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
    public class EmailService : IIdentityMessageService
    {
        private NetworkCredential credentials;
        private string hostEmail;

        public EmailService(NetworkCredential credentials, string hostEmail)
        {
            this.credentials = credentials;
            this.hostEmail = hostEmail;
        }

        public Task SendAsync(IdentityMessage message)
        {
            SendGridMessage mail = new SendGridMessage();

            mail.From = new MailAddress(this.hostEmail, "Voting App");
            mail.AddTo(message.Destination);
            mail.Subject = message.Subject;
            mail.Html = message.Body;

            var transportWeb = new SendGrid.Web(this.credentials);
        
            return transportWeb.DeliverAsync(mail);
        }
    }
}