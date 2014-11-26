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
using System.Threading.Tasks;
using System.Threading;

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
                    if (newSession.TemplateId != 0)
                    {
                        newSession.Options = context.Templates.Where(os => os.Id == newSession.TemplateId).Include(os => os.Options).FirstOrDefault().Options;
                    }
                    else
                    {
                        newSession.Options = new List<Option>();
                    }
                }

                newSession.UUID = Guid.NewGuid();
                newSession.ManageID = Guid.NewGuid();

                // Do the long-running SendEmail task in a different thread, so we can return early
                Thread newThread = new Thread(new ThreadStart(() => SendEmails(newSession)));
                newThread.Start();

                context.Sessions.Add(newSession);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newSession.UUID);
            }
        }

        private void SendEmails(Session session)
        {
            List<string> invitations = session.Invites ?? new List<string>();

            SendCreateEmail(session);
            foreach (string inviteEmail in invitations)
            {
                SendVoteEmail(inviteEmail, session);
            }
        }

        private void SendCreateEmail(Session session)
        {
            string message = String.Join("\n\n", new List<string>()
                {"Your poll is now created and ready to go!",
                 "You can invite people to vote by giving them this link: http://votingapp.azurewebsites.net?poll=" + session.UUID + ".",
                 "You can administer your poll at http://votingapp.azurewebsites.net?manage=" + session.ManageID + ". Don't share this link around!"});

            MailSender.SendMail(session.Email, "Your poll is ready!", message);
        }

        private void SendVoteEmail(string targetEmailAddress, Session session)
        {
            string message = String.Join("\n\n", new List<string>()
                {"You've been invited by " + session.Creator + " to vote on " + session.Name + ".",
                 "Have your say at: http://votingapp.azurewebsites.net?poll=" + session.UUID + "!"});

            MailSender.SendMail(targetEmailAddress, "Have your say!", message);
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
                    if (newSession.TemplateId != 0)
                    {
                        options = context.Templates.Where(os => os.Id == newSession.TemplateId).Include(os => os.Options).FirstOrDefault().Options;
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