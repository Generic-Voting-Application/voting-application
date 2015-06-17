using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Configuration;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Services
{
    public class CorrespondenceService : ICorrespondenceService
    {
        private IMailSender _mailSender;
        private string _invitationTemplate = HtmlFromFile("VotingApplication.Web.Api.Resources.InvitationEmailTemplate.html");
        private string _confirmationTemplate = HtmlFromFile("VotingApplication.Web.Api.Resources.ConfirmEmailTemplate.html");

        public CorrespondenceService(IMailSender mailSender)
        {
            _mailSender = mailSender;
        }

        private static string HtmlFromFile(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
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
            if (String.IsNullOrWhiteSpace(ballot.Email))
            {
                return;
            }

            string hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (String.IsNullOrWhiteSpace(hostUri))
            {
                return;
            }

            string link = hostUri + "/Poll/#/Vote/" + UUID + "/" + ballot.TokenGuid;

            string htmlMessage = (string)_invitationTemplate;
            htmlMessage = htmlMessage.Replace("__VOTEURI__", link);
            htmlMessage = htmlMessage.Replace("__HOSTURI__", hostUri);
            htmlMessage = htmlMessage.Replace("__POLLQUESTION__", pollQuestion);

            _mailSender.SendMail(ballot.Email, "Have your say", htmlMessage);
        }


        public void SendConfirmation(string email, string authenticationToken)
        {
            string hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (String.IsNullOrWhiteSpace(hostUri))
            {
                return;
            }

            var confirmationUrl = new StringBuilder();
            confirmationUrl.Append(hostUri);
            confirmationUrl.Append("/api/Account/ConfirmEmail");
            confirmationUrl.Append("?email=");
            confirmationUrl.Append(HttpUtility.UrlEncode(email));
            confirmationUrl.Append("&code=");

            confirmationUrl.Append(HttpUtility.UrlEncode(authenticationToken));

            string confirmUri = confirmationUrl.ToString();

            string htmlMessage = (string)_confirmationTemplate;

            htmlMessage = htmlMessage.Replace("__HOSTURI__", hostUri);
            htmlMessage = htmlMessage.Replace("__CONFIRMURI__", confirmUri);

            _mailSender.SendMail(email, "Confirm your Email", htmlMessage);
        }
    }
}