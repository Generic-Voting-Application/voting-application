using System;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Services
{
    public class InvitationService : IInvitationService
    {
        private IMailSender _mailSender;
        private string _htmlTemplate = HtmlFromFile("VotingApplication.Web.Api.Resources.EmailTemplate.html");

        public InvitationService(IMailSender mailSender)
        {
            _mailSender = mailSender;
        }

        private static string HtmlFromFile(string path)
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

        public void SendInvitation(Guid UUID, Ballot ballot, string pollQuestion)
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

            string htmlMessage = (string)_htmlTemplate.Clone();
            htmlMessage = htmlMessage.Replace("__VOTEURI__", link);
            htmlMessage = htmlMessage.Replace("__HOSTURI__", hostUri);
            htmlMessage = htmlMessage.Replace("__POLLQUESTION__", pollQuestion);

            _mailSender.SendMail(ballot.Email, "Have your say", htmlMessage);
        }

    }
}