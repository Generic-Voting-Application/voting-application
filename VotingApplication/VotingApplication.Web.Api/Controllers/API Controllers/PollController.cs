using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Configuration;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollController : WebApiController
    {
        private IMailSender _mailSender;

        public PollController()
            : base()
        {
            _mailSender = new MailSender();
        }
        public PollController(IContextFactory contextFactory, IMailSender mailSender)
            : base(contextFactory)
        {
            _mailSender = mailSender;
        }

        #region Get

        public override HttpResponseMessage Get()
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use GET on this controller");
        }

        public virtual HttpResponseMessage Get(Guid id)
        {
            #region DB Get / Validation

            Poll poll;
            using (var context = _contextFactory.CreateContext())
            {
                poll = context.Polls.Where(s => s.UUID == id).Include(s => s.Options).FirstOrDefault();
            }

            if (poll == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Poll {0} not found", id));
            }

            #endregion

            #region Response

            PollRequestResponseModel response = new PollRequestResponseModel();

            response.Name = poll.Name;
            response.Creator = poll.Creator;
            response.VotingStrategy = poll.VotingStrategy;
            response.MaxPoints = poll.MaxPoints;
            response.MaxPerVote = poll.MaxPerVote;
            response.InviteOnly = poll.InviteOnly;
            response.AnonymousVoting = poll.AnonymousVoting;
            response.RequireAuth = poll.RequireAuth;
            response.Expires = poll.Expires;
            response.ExpiryDate = poll.ExpiryDate;
            response.OptionAdding = poll.OptionAdding;
            response.Options = poll.Options;

            return this.Request.CreateResponse(HttpStatusCode.OK, response);

            #endregion
        }

        #endregion

        #region Put

        public virtual HttpResponseMessage Put(object obj)
        {
            return this.Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Cannot use PUT on this controller");
        }

        #endregion

        #region Post

        public virtual HttpResponseMessage Post(PollCreationRequestModel pollCreationRequest)
        {
            #region Input Validation

            if (pollCreationRequest == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            if (pollCreationRequest.Expires && pollCreationRequest.ExpiryDate < DateTime.Now)
            {
                ModelState.AddModelError("ExpiryDate", "Invalid or unspecified ExpiryDate");
            }

            if (!ModelState.IsValid)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            #region DB Object Creation

            Poll newPoll = new Poll();

            newPoll.UUID = Guid.NewGuid();
            newPoll.ManageId = Guid.NewGuid();
            newPoll.Name = pollCreationRequest.Name;
            newPoll.Creator = pollCreationRequest.Creator;
            newPoll.VotingStrategy = pollCreationRequest.VotingStrategy;
            newPoll.TemplateId = 0;
            newPoll.Options = new List<Option>();
            newPoll.MaxPoints = pollCreationRequest.MaxPoints;
            newPoll.MaxPerVote = pollCreationRequest.MaxPerVote;
            newPoll.InviteOnly = pollCreationRequest.InviteOnly;
            newPoll.Tokens = new List<Token>();
            newPoll.ChatMessages = new List<ChatMessage>();
            newPoll.AnonymousVoting = pollCreationRequest.AnonymousVoting;
            newPoll.RequireAuth = pollCreationRequest.RequireAuth;
            newPoll.Expires = pollCreationRequest.Expires;
            newPoll.ExpiryDate = pollCreationRequest.ExpiryDate;
            newPoll.OptionAdding = pollCreationRequest.OptionAdding;
            newPoll.LastUpdated = DateTime.Now;

            using (var context = _contextFactory.CreateContext())
            {
                context.Polls.Add(newPoll);
                context.SaveChanges();
            }

            #endregion

            Thread newThread = new Thread(new ThreadStart(() => SendCreateEmail(pollCreationRequest.Email, newPoll.UUID, newPoll.ManageId)));
            newThread.Start();

            #region Response

            PollCreationResponseModel response = new PollCreationResponseModel();

            response.UUID = newPoll.UUID;
            response.ManageId = newPoll.ManageId;

            return this.Request.CreateResponse(HttpStatusCode.OK, response);

            #endregion
        }

        private void SendCreateEmail(string email, Guid UUID, Guid manageId)
        {
            String hostUri = WebConfigurationManager.AppSettings["HostURI"];
            if (hostUri == String.Empty)
            {
                return;
            }

            string message = String.Join("\n\n", new List<string>()
                {"Your poll is now created and ready to go!",
                 "You can invite people to vote by giving them this link: " + hostUri + "/Poll/Index/" + UUID,
                 "You can administer your poll at "+ hostUri + "/Manage/Index/" + manageId,
                 "(Don't share this link around!)"});

            _mailSender.SendMail(email, "Your poll is ready!", message);
        }

        #endregion
    }
}