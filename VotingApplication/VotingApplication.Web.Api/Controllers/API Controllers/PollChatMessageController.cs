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
using System.Web.OData;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollChatMessageController : WebApiController
    {
        public PollChatMessageController() : base() { }
        public PollChatMessageController(IContextFactory contextFactory) : base(contextFactory) { }

        #region Get

        public override HttpResponseMessage Get()
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET on this controller");
        }

        [EnableQuery]
        public virtual HttpResponseMessage Get(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(p => p.UUID == pollId).Include(p => p.ChatMessages).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", pollId));
                }

                List<ChatMessage> messages = matchingPoll.ChatMessages ??  new List<ChatMessage>();

                return this.Request.CreateResponse(HttpStatusCode.OK, messages);
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

        public virtual HttpResponseMessage Post(Guid pollId, ChatMessage newMessage)
        {
            using (var context = _contextFactory.CreateContext())
            {
                if (newMessage == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid message");
                }

                if(newMessage.User == null || newMessage.User.Id == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Message requires a user");
                }

                if (newMessage.Message == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Message text required");
                }

                User matchingUser = context.Users.Where(u => u.Id == newMessage.User.Id).FirstOrDefault();
                if (matchingUser == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("User {0} does not exist", newMessage.User.Id));
                }

                Poll matchingPoll = context.Polls.Where(p => p.UUID == pollId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} does not exist", pollId));
                }

                newMessage.User = matchingUser;
                newMessage.Timestamp = DateTime.Now;
                if (matchingPoll.ChatMessages == null)
                {
                    matchingPoll.ChatMessages = new List<ChatMessage>();
                }
                matchingPoll.ChatMessages.Add(newMessage);
                context.SaveChanges();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion
    }
}