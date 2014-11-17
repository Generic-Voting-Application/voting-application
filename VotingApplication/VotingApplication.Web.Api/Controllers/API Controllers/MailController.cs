using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security;
using System.Web;
using System.Web.Configuration;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class MailController : WebApiController
    {
        public virtual HttpResponseMessage Post(Dictionary<String, String> input)
        {
            string hostEmail = WebConfigurationManager.AppSettings["HostEmail"];
            string hostPassword = WebConfigurationManager.AppSettings["HostPassword"];

            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "outlook.office365.com";
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(hostEmail, hostPassword);

            MailMessage mail = new MailMessage(hostEmail, input["email"]);

            string messageBody =
                "Your poll is now created and ready to go!\n\n" +
                "You can invite people to vote by giving them this link: http://voting-app.azurewebsites.net?poll=" + input["PollId"] + ".\n\n" +
                "You can administer your poll at http://voting-app.azurewebsites.net?poll=" + input["ManageId"] + ". Don't share this link around!";

            mail.Subject = "Your poll is ready!";
            mail.Body = messageBody;

            client.Send(mail);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}