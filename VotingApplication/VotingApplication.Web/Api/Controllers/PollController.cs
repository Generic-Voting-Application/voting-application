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

                Ballot ballot;
                if (tokenGuid.HasValue)
                {
                    ballot = poll.Ballots.SingleOrDefault(b => b.TokenGuid == tokenGuid.Value);

                    if (ballot == null)
                    {
                        ThrowError(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    tokenGuid = Guid.NewGuid();

                    ballot = new Ballot()
                    {
                        TokenGuid = tokenGuid.Value

                    };

                    poll.Ballots.Add(ballot);
                    context.Ballots.Add(ballot);

                    context.SaveChanges();
                }

                return CreateResponse(poll, tokenGuid.Value, ballot);
            }
        }

        private static PollRequestResponseModel CreateResponse(Poll poll, Guid tokenGuid, Ballot ballot)
        {
            return new PollRequestResponseModel
            {
                Name = poll.Name,
                PollType = poll.PollType.ToString(),
                ExpiryDateUtc = poll.ExpiryDateUtc,

                MaxPoints = poll.MaxPoints,
                MaxPerVote = poll.MaxPerVote,

                TokenGuid = tokenGuid,

                Choices = CreateChoiceResponse(poll.Choices, ballot.Votes),

                NamedVoting = poll.NamedVoting,
                ChoiceAdding = poll.ChoiceAdding,
                ElectionMode = poll.ElectionMode,

                UserHasVoted = ballot.HasVoted
            };
        }

        private static IEnumerable<PollRequestChoiceResponseModel> CreateChoiceResponse(IEnumerable<Choice> choices, IEnumerable<Vote> votes)
        {
            IEnumerable<PollRequestChoiceResponseModel> responses =
                    from choice in choices
                    join vote in votes on choice equals vote.Choice into voteChoices

                    from voteChoice in voteChoices.DefaultIfEmpty()
                    select new PollRequestChoiceResponseModel()
                    {
                        Id = choice.Id,
                        Name = choice.Name,
                        Description = choice.Description,
                        PollChoiceNumber = choice.PollChoiceNumber,
                        VoteValue = (voteChoice == null ? 0 : voteChoice.VoteValue)
                    };

            return responses.ToList();
        }

        [HttpPost]
        public PollCreationResponseModel Post(PollCreationRequestModel pollCreationRequest)
        {
            #region Input Validation

            if (pollCreationRequest == null)
            {
                ThrowError(HttpStatusCode.BadRequest);
            }

            PollType pollType;
            if (!Enum.TryParse<PollType>(pollCreationRequest.PollType, true, out pollType))
            {
                ModelState.AddModelError("PollType", "Invalid PollType");
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
                newPoll.ChoiceAdding = pollCreationRequest.ChoiceAdding;
                newPoll.PollType = (PollType)Enum.Parse(typeof(PollType), pollCreationRequest.PollType);
                newPoll.NamedVoting = pollCreationRequest.NamedVoting;
                newPoll.ElectionMode = pollCreationRequest.ElectionMode;
                newPoll.ExpiryDateUtc = pollCreationRequest.ExpiryDateUtc;
                newPoll.InviteOnly = pollCreationRequest.InviteOnly;
            };

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