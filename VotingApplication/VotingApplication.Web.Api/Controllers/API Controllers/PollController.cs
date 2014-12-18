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
    public class PollController : WebApiController
    {
        public PollController() : base() { }
        public PollController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get

        public override HttpResponseMessage Get()
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET on this controller");
        }

        public virtual HttpResponseMessage Get(Guid id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.UUID == id).Include(s => s.Options).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", id));
                }

                // Hide the manageID to prevent a GET on the poll ID from giving Poll Creator access
                matchingPoll.ManageID = Guid.Empty;

                // Similarly with tokens
                matchingPoll.Tokens = new List<Token>();

                return this.Request.CreateResponse(HttpStatusCode.OK, matchingPoll);
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

        public virtual HttpResponseMessage Post(Poll newPoll)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newPoll.Name == null || newPoll.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Poll did not have a name");
                }

                if (newPoll.Options == null)
                {
                    if (newPoll.TemplateId != 0)
                    {
                        newPoll.Options = context.Templates.Where(os => os.Id == newPoll.TemplateId).Include(os => os.Options).FirstOrDefault().Options;
                    }
                    else
                    {
                        newPoll.Options = new List<Option>();
                    }
                }

                // Create a list of tokens for each invite
                if(newPoll.InviteOnly)
                {
                    newPoll.Tokens = new List<Token>();
                    foreach(string email in newPoll.Invites)
                    {
                        newPoll.Tokens.Add(new Token { TokenGuid = Guid.NewGuid() });
                    }
                }

                newPoll.UUID = Guid.NewGuid();
                newPoll.ManageID = Guid.NewGuid();

                // Do the long-running SendEmail task in a different thread, so we can return early
                Thread newThread = new Thread(new ThreadStart(() => SendEmails(newPoll)));
                newThread.Start();

                context.Polls.Add(newPoll);
                context.SaveChanges();

                Poll returnData = new Poll() { UUID = newPoll.UUID, ManageID = newPoll.ManageID };

                return this.Request.CreateResponse(HttpStatusCode.OK, returnData);
            }
        }

        private void SendEmails(Poll poll)
        {
            List<string> invitations = poll.Invites ?? new List<string>();
            Queue<Token> tokens = poll.InviteOnly ? new Queue<Token>(poll.Tokens) : null;

            SendCreateEmail(poll);
            foreach(string invitation in invitations)
            {
                SendVoteEmail(invitation, poll, poll.InviteOnly ? tokens.Dequeue() : new Token { TokenGuid = Guid.Empty });
            }
        }

        private void SendCreateEmail(Poll poll)
        {
            String hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (hostUri == String.Empty)
            {
                return;
            }

            string message = String.Join("\n\n", new List<string>()
                {"Your poll is now created and ready to go!",
                 "You can invite people to vote by giving them this link: " + hostUri + "?poll=" + poll.UUID,
                 "You can administer your poll at "+ hostUri + "?manage=" + poll.ManageID,
                 "(Don't share this link around!)"});

            MailSender.SendMail(poll.Email, "Your poll is ready!", message);
        }

        private void SendVoteEmail(string targetEmailAddress, Poll poll, Token token)
        {
            String hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (hostUri == String.Empty)
            {
                return;
            }

            string tokenString = (poll.InviteOnly && token != null && token.TokenGuid != Guid.Empty) ? "&token=" + token.TokenGuid : "";

            string message = String.Join("\n\n", new List<string>()
                {"You've been invited by " + poll.Creator + " to vote on " + poll.Name,
                 "Have your say at: " + hostUri + "?poll=" + poll.UUID + tokenString});

            MailSender.SendMail(targetEmailAddress, "Have your say!", message);
        }

        public virtual HttpResponseMessage Post(Guid id, Poll newPoll)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Poll is null");
                }

                if (newPoll.Name == null || newPoll.Name.Length == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Poll does not have a name");
                }

                if (newPoll.Options == null)
                {
                    List<Option> options = new List<Option>();
                    if (newPoll.TemplateId != 0)
                    {
                        options = context.Templates.Where(os => os.Id == newPoll.TemplateId).Include(os => os.Options).FirstOrDefault().Options;
                    }

                    newPoll.Options = options;
                }

                Poll matchingPoll = context.Polls.Where(s => s.UUID == id).FirstOrDefault();
                if (matchingPoll != null)
                {
                    context.Polls.Remove(matchingPoll);
                }

                context.Polls.Add(newPoll);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK, newPoll.UUID);
            }
        }

        #endregion
    }
}