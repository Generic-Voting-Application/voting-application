﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Configuration;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models;

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
            if (string.IsNullOrWhiteSpace(ballot.Email))
            {
                return;
            }

            string hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (string.IsNullOrWhiteSpace(hostUri))
            {
                return;
            }

            string link = hostUri + "/Poll/#/Vote/" + UUID + "/" + ballot.TokenGuid;

            string htmlMessage = (string)_invitationTemplate.Clone();
            htmlMessage = htmlMessage.Replace("__VOTEURI__", link);
            htmlMessage = htmlMessage.Replace("__HOSTURI__", hostUri);
            htmlMessage = htmlMessage.Replace("__POLLQUESTION__", pollQuestion);

            _mailSender.SendMail(ballot.Email, "Have your say", htmlMessage);
        }


        public void SendConfirmation(ApplicationUser user, string code)
        {
            string hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (string.IsNullOrWhiteSpace(hostUri))
            {
                return;
            }

            StringBuilder confirmationUrl = new StringBuilder();
            confirmationUrl.Append(hostUri);
            confirmationUrl.Append("/Account/ConfirmEmail");
            confirmationUrl.Append("?userId=");
            confirmationUrl.Append(user.Id);
            confirmationUrl.Append("&code=");
            confirmationUrl.Append(HttpUtility.HtmlEncode(code));

            string htmlMessage = (string)_confirmationTemplate.Clone();

            htmlMessage = htmlMessage.Replace("__HOSTURI__", hostUri);
            htmlMessage = htmlMessage.Replace("__CONFIRMURI__", confirmationUrl.ToString());

            _mailSender.SendMail(user.Email, "Confirm your Email", htmlMessage);
        }
    }
}