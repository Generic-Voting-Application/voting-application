using SendGrid;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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

            mail.From = new MailAddress(this.hostEmail, "Vote On");
            mail.AddTo(to);
            mail.Subject = subject;
            mail.Html = message;

            var transportWeb = new SendGrid.Web(this.credentials);

            return transportWeb.DeliverAsync(mail);
        }
    }
}