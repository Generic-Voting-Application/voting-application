using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
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
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                Poll poll = PollByPollId(id, context);

                Guid? tokenGuid = GetTokenGuidFromHeaders();
                bool userHasVoted = false;

                if (poll.InviteOnly)
                {
                    if (!tokenGuid.HasValue)
                    {
                        ThrowError(HttpStatusCode.Unauthorized);
                    }

                    if (poll.Ballots.All(b => b.TokenGuid != tokenGuid.Value))
                    {
                        ThrowError(HttpStatusCode.Unauthorized);
                    }
                }

                if (tokenGuid.HasValue)
                {
                    Ballot ballot = poll.Ballots.Where(b => b.TokenGuid == tokenGuid.Value).SingleOrDefault();

                    if (ballot == null)
                    {
                        ThrowError(HttpStatusCode.NotFound);
                    }

                    userHasVoted = ballot.HasVoted;
                }
                else
                {
                    tokenGuid = Guid.NewGuid();

                    var ballot = new Ballot()
                    {
                        TokenGuid = tokenGuid.Value

                    };

                    poll.Ballots.Add(ballot);
                    context.Ballots.Add(ballot);

                    context.SaveChanges();
                }

                return CreateResponse(poll, tokenGuid.Value, userHasVoted);
            }
        }

        private Guid? GetTokenGuidFromHeaders()
        {
            IEnumerable<string> tokenHeaders;
            bool success = Request.Headers.TryGetValues("X-TokenGuid", out tokenHeaders);

            if (success)
            {
                if (tokenHeaders.Count() > 1)
                {
                    ThrowError(HttpStatusCode.BadRequest, "Multiple X-TokenGuid headers");
                }

                return new Guid(tokenHeaders.Single());
            }

            return null;
        }

        private static PollRequestResponseModel CreateResponse(Poll poll, Guid tokenGuid, bool hasVoted)
        {
            return new PollRequestResponseModel
            {
                Name = poll.Name,
                PollType = poll.PollType.ToString(),
                ExpiryDateUtc = poll.ExpiryDateUtc,

                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,

                TokenGuid = tokenGuid,

                Choices = poll.Choices,

                NamedVoting = poll.NamedVoting,
                ChoiceAdding = poll.ChoiceAdding,
                ElectionMode = poll.ElectionMode,

                UserHasVoted = hasVoted
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
            if (pollCreationRequest.Choices != null &&
               pollCreationRequest.Choices.Count > 0)
            {
                newPoll.Choices = pollCreationRequest.Choices;
            }

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