using Microsoft.AspNet.Identity;
using System;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Data.Model.Creation;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollController : WebApiController
    {
        public PollController() { }

        public PollController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        public PollRequestResponseModel Get(Guid id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByPollId(id, context);
                return CreatePollResponseFromModel(poll);
            }
        }

        private static PollRequestResponseModel CreatePollResponseFromModel(Poll poll)
        {
            return new PollRequestResponseModel
            {
                Name = poll.Name,
                PollType = poll.PollType.ToString(),
                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,
                NamedVoting = poll.NamedVoting,
                ExpiryDateUtc = poll.ExpiryDateUtc,
                ChoiceAdding = poll.ChoiceAdding,
                Choices = poll.Choices,
                IsElectionMode = poll.IsElectionMode
            };
        }

        [HttpPost]
        public PollCreationResponseModel Post(PollCreationRequestModel pollCreationRequest)
        {
            #region Input Validation

            if (pollCreationRequest == null)
            {
                ThrowError(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            #endregion

            Poll newPoll = Create(pollCreationRequest);
            Ballot creatorBallot = new Ballot
            {
                TokenGuid = Guid.NewGuid(),
                ManageGuid = Guid.NewGuid()
            };

            using (var context = _contextFactory.CreateContext())
            {
                _metricHandler.HandlePollCreatedEvent(newPoll);

                context.Polls.Add(newPoll);
                newPoll.Ballots.Add(creatorBallot);

                context.SaveChanges();

            }

            PollCreationResponseModel response = new PollCreationResponseModel
            {
                UUID = newPoll.UUID,
                ManageId = newPoll.ManageId,
                CreatorBallot = creatorBallot
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