using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using System.Net.Mail;
using System.Web.Configuration;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class SessionController : WebApiController
    {
        public SessionController() : base() { }
        public SessionController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get

        public override HttpResponseMessage Get()
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET on this controller");
        }

        public virtual HttpResponseMessage Get(Guid id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Session matchingSession = context.Sessions.Where(s => s.UUID == id).Include(s => s.Options).FirstOrDefault();
                if (matchingSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Session {0} does not exist", id));
                }

                // Hide the manageID to prevent a GET on the poll ID from giving Poll Creator access
                matchingSession.ManageID = Guid.Empty;

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingSession);
            }
        }

        #endregion

        #region Put

        public virtual HttpResponseMessage Put(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(Session newSession)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newSession.Name == null || newSession.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Session did not have a name");
                }

                if (newSession.Options == null)
                {
                    if (newSession.OptionSetId != 0)
                    {
                        newSession.Options = context.OptionSets.Where(os => os.Id == newSession.OptionSetId).Include(os => os.Options).FirstOrDefault().Options;
                    }
                    else
                    {
                        newSession.Options = new List<Option>();
                    }
                }

                newSession.UUID = Guid.NewGuid();
                newSession.ManageID = Guid.NewGuid();

                string targetEmail = newSession.Email;
                newSession.Email = null; // Don't store email longer than we need to

                SendEmail(targetEmail, newSession.UUID, newSession.ManageID);

                context.Sessions.Add(newSession);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newSession.UUID);
            }
        }

        private void SendEmail(string targetEmailAddress, Guid pollId, Guid manageId)
        {
            string hostEmail = WebConfigurationManager.AppSettings["HostEmail"];
            string hostPassword = WebConfigurationManager.AppSettings["HostPassword"];

            if (hostEmail == null || hostPassword == null)
            {
                return;
            }

            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "outlook.office365.com";
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(hostEmail, hostPassword);

            MailMessage mail = new MailMessage(hostEmail, targetEmailAddress);

            string messageBody =
@"Your poll is now created and ready to go!

You can invite people to vote by giving them this link: http://voting-app.azurewebsites.net?poll=" + pollId + @"

You can administer your poll at http://voting-app.azurewebsites.net?manage=" + manageId + ". Don't share this link around!";

            mail.Subject = "Your poll is ready!";
            mail.Body = messageBody;

            try
            {
                client.Send(mail);
            }
            catch (SmtpException e)
            {
                // Do nothing
            }
        }

        public virtual HttpResponseMessage Post(Guid id, Session newSession)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newSession == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Session is null");
                }

                if (newSession.Name == null || newSession.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Session does not have a name");
                }

                if (newSession.Options == null)
                {
                    List<Option> options = new List<Option>();
                    if (newSession.OptionSetId != 0)
                    {
                        options = context.OptionSets.Where(os => os.Id == newSession.OptionSetId).Include(os => os.Options).FirstOrDefault().Options;
                    }

                    newSession.Options = options;
                }

                Session matchingSession = context.Sessions.Where(s => s.UUID == id).FirstOrDefault();
                if (matchingSession != null)
                {
                    context.Sessions.Remove(matchingSession);
                }

                context.Sessions.Add(newSession);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newSession.UUID);
            }
        }

        #endregion
    }
}