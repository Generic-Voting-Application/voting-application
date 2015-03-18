using Microsoft.AspNet.Identity;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;

namespace VotingApplication.Web.Api.Services
{
    public class Smtp4DevEmailSender : IMailSender
    {
        private string hostEmail;

        public Smtp4DevEmailSender(string hostEmail)
        {
            this.hostEmail = hostEmail;
        }

        public Task SendMail(string to, string subject, string message)
        {
            string text = message;
            string html = message;

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(this.hostEmail, "Voting App");
            msg.To.Add(new MailAddress(to));
            msg.Subject = subject;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            try
            {
                SmtpClient smtpClient = new SmtpClient("localhost");
                smtpClient.Send(msg);
            }
            catch 
            {
            }


            return Task.FromResult(0);
        }
    }
}