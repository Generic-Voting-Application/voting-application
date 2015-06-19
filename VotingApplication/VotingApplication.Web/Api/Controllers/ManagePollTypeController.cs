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
    public class ManagePollTypeController : WebApiController
    {
        public ManagePollTypeController() : base() { }

        public ManagePollTypeController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        [HttpPut]
        public void Put(Guid manageId, ManagePollTypeRequest updateRequest)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                PollType pollType;
                if (!Enum.TryParse<PollType>(updateRequest.PollType, true, out pollType))
                {
                    ModelState.AddModelError("PollType", "Invalid PollType");
                }

                if (!ModelState.IsValid)
                {
                    ThrowError(HttpStatusCode.BadRequest, ModelState);
                }

                if (poll.PollType == pollType)
                {
                    if (pollType != PollType.Points || (poll.MaxPoints == updateRequest.MaxPoints && poll.MaxPerVote == updateRequest.MaxPerVote))
                    {
                        return;
                    }
                }

                List<Vote> removedVotes = context.Votes.Include(v => v.Poll)
                                                        .Where(v => v.Poll.UUID == poll.UUID)
                                                        .ToList();
                foreach (Vote oldVote in removedVotes)
                {
                    _metricHandler.HandleVoteDeletedEvent(oldVote, poll.UUID);
                    context.Votes.Remove(oldVote);
                }

                _metricHandler.HandlePollTypeChangedEvent(pollType, updateRequest.MaxPerVote ?? 0, updateRequest.MaxPoints ?? 0, poll.UUID);

                poll.PollType = pollType;
                poll.MaxPerVote = updateRequest.MaxPerVote;

                poll.MaxPoints = updateRequest.MaxPoints;

                poll.LastUpdatedUtc = DateTime.UtcNow;

                context.SaveChanges();
            }
        }

        [HttpGet]
        public ManagePollTypeRequestResponse Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByManageId(manageId, context);

                return pollToModel(poll);
            }
        }

        private ManagePollTypeRequestResponse pollToModel(Poll poll)
        {
            var model = new ManagePollTypeRequestResponse()
            {
                MaxPerVote = poll.MaxPerVote,
                MaxPoints = poll.MaxPoints,
                PollType = poll.PollType.ToString()
            };

            model.PollHasVotes = poll.Ballots.Any(b => b.HasVoted);

            return model;
        }
    }
}
