using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Configuration;
using System.Web.Http;
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

        private PollRequestResponseModel PollToModel(Poll poll)
        {
            return new PollRequestResponseModel
            {
                UUID = poll.UUID,
                Name = poll.Name,
                Creator = poll.Creator,
                VotingStrategy = poll.PollType.ToString(),
                CreatedDate = poll.CreatedDate,
                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,
                InviteOnly = poll.InviteOnly,
                AnonymousVoting = poll.AnonymousVoting,
                RequireAuth = poll.RequireAuth,
                Expires = poll.Expires,
                ExpiryDate = poll.ExpiryDate,
                OptionAdding = poll.OptionAdding,
                Options = poll.Options
            };
        }

        private Poll ModelToPoll(PollCreationRequestModel pollCreationRequest)
        {
            return new Poll
            {
                UUID = Guid.NewGuid(),
                ManageId = Guid.NewGuid(),
                Name = pollCreationRequest.Name,
                Creator = pollCreationRequest.Creator,
                PollType = pollCreationRequest.VotingStrategy != null && 
                           Enum.IsDefined(typeof(PollType), pollCreationRequest.VotingStrategy) ?
                                (PollType)Enum.Parse(typeof(PollType), pollCreationRequest.VotingStrategy, true) : PollType.Basic,
                Options = new List<Option>(),
                MaxPoints = pollCreationRequest.MaxPoints,
                MaxPerVote = pollCreationRequest.MaxPerVote,
                InviteOnly = pollCreationRequest.InviteOnly,
                Tokens = new List<Token>(),
                ChatMessages = new List<ChatMessage>(),
                AnonymousVoting = pollCreationRequest.AnonymousVoting,
                RequireAuth = pollCreationRequest.RequireAuth,
                Expires = pollCreationRequest.Expires,
                ExpiryDate = pollCreationRequest.ExpiryDate,
                OptionAdding = pollCreationRequest.OptionAdding,
                LastUpdated = DateTime.Now,
                CreatedDate = DateTime.Now,
                CreatorIdentity = this.User.Identity.Name
            };
        }

        #region Get

        [Authorize]
        public List<PollRequestResponseModel> Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                var username = this.User.Identity.Name;
                List<Poll> matchingPolls = context.Polls.Where(p => p.CreatorIdentity == username).ToList<Poll>();
                return matchingPolls.Select(p => PollToModel(p)).ToList();
            }
        }

        public PollRequestResponseModel Get(Guid id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.UUID == id).Include(s => s.Options).FirstOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", id));
                }

                return PollToModel(poll);
            }
        }

        #endregion

        #region Post

        public PollCreationResponseModel Post(PollCreationRequestModel pollCreationRequest)
        {
            #region Input Validation

            if (pollCreationRequest == null)
            {
                this.ThrowError(HttpStatusCode.BadRequest);
            }

            if (pollCreationRequest.Expires && pollCreationRequest.ExpiryDate < DateTime.Now)
            {
                ModelState.AddModelError("ExpiryDate", "Invalid or unspecified ExpiryDate");
            }

            if (!ModelState.IsValid)
            {
                this.ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            #region DB Object Creation

            Poll newPoll = ModelToPoll(pollCreationRequest);

            using (var context = _contextFactory.CreateContext())
            {
                context.Polls.Add(newPoll);
                context.SaveChanges();
            }

            #endregion

            Thread newThread = new Thread(new ThreadStart(() => SendCreateEmail(pollCreationRequest.Email, newPoll.UUID, newPoll.ManageId)));
            newThread.Start();

            #region Response

            PollCreationResponseModel response = new PollCreationResponseModel
            {
                UUID = newPoll.UUID,
                ManageId = newPoll.ManageId
            };

            return response;

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