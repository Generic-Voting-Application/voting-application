using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class PollController : WebApiController
    {
        public PollController()
        {
        }
        public PollController(IContextFactory contextFactory)
            : base(contextFactory)
        {
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
                NamedVoting = poll.NamedVoting,
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
                NamedVoting = pollCreationRequest.NamedVoting,
                RequireAuth = pollCreationRequest.RequireAuth,
                Expires = pollCreationRequest.Expires,
                ExpiryDate = pollCreationRequest.ExpiryDate,
                OptionAdding = pollCreationRequest.OptionAdding,
                LastUpdated = DateTime.Now,
                CreatedDate = DateTime.Now,
                CreatorIdentity = User.Identity.Name
            };
        }

        #region Get

        [Authorize]
        public List<PollRequestResponseModel> Get()
        {
            using (var context = _contextFactory.CreateContext())
            {
                var username = User.Identity.Name;
                List<Poll> matchingPolls = context.Polls.Where(p => p.CreatorIdentity == username).ToList();
                return matchingPolls.Select(PollToModel).ToList();
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

        [HttpPost]
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

            Poll newPoll = ModelToPoll(pollCreationRequest);

            using (var context = _contextFactory.CreateContext())
            {
                context.Polls.Add(newPoll);
                context.SaveChanges();
            }

            PollCreationResponseModel response = new PollCreationResponseModel
            {
                UUID = newPoll.UUID,
                ManageId = newPoll.ManageId
            };

            return response;
        }
    }
}