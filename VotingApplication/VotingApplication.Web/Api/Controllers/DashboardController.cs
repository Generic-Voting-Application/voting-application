using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers
{
    public class DashboardController : WebApiController
    {
        public DashboardController() { }

        public DashboardController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpGet]
        [Authorize]
        public List<DashboardPollResponseModel> Polls()
        {
            using (IVotingContext context = _contextFactory.CreateContext())
            {
                string userId = User.Identity.GetUserId();

                IEnumerable<DashboardPollResponseModel> userPolls = context
                    .Polls
                    .Where(p => p.CreatorIdentity == userId)
                    .OrderByDescending(p => p.CreatedDateUtc)
                    .Include(p => p.Choices)
                    .Select(CreateDashboardResponseFromModel);

                return userPolls.ToList();
            }
        }

        private static DashboardPollResponseModel CreateDashboardResponseFromModel(Poll poll)
        {
            return new DashboardPollResponseModel()
            {
                UUID = poll.UUID,
                ManageId = poll.ManageId,
                Name = poll.Name,
                CreatedDateUtc = poll.CreatedDateUtc,
                ChoiceCount = poll.Choices.Count,
                ExpiryDateUtc = poll.ExpiryDateUtc,
                PollType = poll.PollType.ToString()
            };
        }


        [HttpPost]
        [Authorize]
        public CopyPollResponseModel Copy(CopyPollRequestModel pollCopyRequest)
        {
            if (pollCopyRequest == null)
            {
                ThrowError(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                ThrowError(HttpStatusCode.BadRequest, ModelState);
            }

            using (IVotingContext context = _contextFactory.CreateContext())
            {
                string userId = User.Identity.GetUserId();

                Poll pollToCopy = context
                    .Polls
                    .Include(p => p.Choices)
                    .SingleOrDefault(p => p.UUID == pollCopyRequest.UUIDToCopy);

                if (pollToCopy == null)
                {
                    ThrowError(HttpStatusCode.BadRequest);
                }


                if (pollToCopy.CreatorIdentity != userId)
                {
                    ThrowError(HttpStatusCode.Forbidden);
                }

                Poll newPoll = CopyPoll(pollToCopy);

                _metricHandler.HandlePollClonedEvent(newPoll);

                Ballot creatorBallot = CreateBallotForCreator();
                newPoll.Ballots.Add(creatorBallot);

                context.Polls.Add(newPoll);
                context.SaveChanges();

                return new CopyPollResponseModel()
                {
                    NewPollId = newPoll.UUID,
                    NewManageId = newPoll.ManageId,
                    CreatorBallotToken = creatorBallot.TokenGuid
                };
            }
        }

        private static Poll CopyPoll(Poll pollToCopy)
        {
            return new Poll()
            {
                UUID = Guid.NewGuid(),
                ManageId = Guid.NewGuid(),
                Name = "Copy of " + pollToCopy.Name,

                Creator = pollToCopy.Creator,
                CreatorIdentity = pollToCopy.CreatorIdentity,

                PollType = pollToCopy.PollType,

                MaxPoints = pollToCopy.MaxPoints,
                MaxPerVote = pollToCopy.MaxPerVote,

                InviteOnly = pollToCopy.InviteOnly,
                NamedVoting = pollToCopy.NamedVoting,
                ChoiceAdding = pollToCopy.ChoiceAdding,

                ExpiryDateUtc = pollToCopy.ExpiryDateUtc,
                LastUpdatedUtc = DateTime.UtcNow,
                CreatedDateUtc = DateTime.UtcNow,


                Choices = CopyOptions(pollToCopy.Choices)
            };
        }

        private static List<Choice> CopyOptions(IEnumerable<Choice> options)
        {
            if (options == null)
            {
                return new List<Choice>(0);
            }

            return options
                .Select(o => new Choice()
                {
                    Name = o.Name,
                    Description = o.Description
                })
                .ToList();
        }

        private static Ballot CreateBallotForCreator()
        {
            return new Ballot
            {
                TokenGuid = Guid.NewGuid(),
                ManageGuid = Guid.NewGuid()
            };
        }
    }
}
