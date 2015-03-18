using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Data.Model.Creation;
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
                ExpiryDate = poll.ExpiryDate,
                OptionAdding = poll.OptionAdding,
                Options = poll.Options
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

            if (!ModelState.IsValid)
            {
                this.ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            Poll newPoll = Create(pollCreationRequest);

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

        private Poll Create(PollCreationRequestModel pollCreationRequest)
        {
            Poll newPoll = PollCreationHelper.Create();
            newPoll.Name = pollCreationRequest.PollName;

            if (User.Identity.IsAuthenticated)
            {
                newPoll.Creator = User.Identity.GetUserName();
                newPoll.CreatorIdentity = User.Identity.GetUserId();
            }
            else
            {
                newPoll.Creator = "Anonymous";
                newPoll.CreatorIdentity = null;
            }

            return newPoll;
        }
    }
}