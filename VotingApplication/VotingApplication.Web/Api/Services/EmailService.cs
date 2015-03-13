using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace VotingApplication.Web.Api.Services
{
    public class EmailService : IIdentityMessageService
    {
        private IMailSender mailSender;

        public EmailService(IMailSender mailSender)
        {
            this.mailSender = mailSender;
        }

        public async Task SendAsync(IdentityMessage message)
        {
            await mailSender.SendMail(message.Destination, message.Subject, message.Body);
        }
    }
}