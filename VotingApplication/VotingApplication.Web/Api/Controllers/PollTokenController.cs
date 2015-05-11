using System;
using System.Collections.Generic;
using System.Net;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Metrics;

namespace VotingApplication.Web.Api.Controllers
{
    public class PollTokenController : WebApiController
    {
        public PollTokenController() : base() { }
        public PollTokenController(IContextFactory contextFactory, IMetricHandler metricHandler) : base(contextFactory, metricHandler) { }

        #region GET

        public Guid Get(Guid pollId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = PollByPollId(pollId, context);

                if (poll.InviteOnly)
                {
                    ThrowError(HttpStatusCode.Forbidden, string.Format("Poll {0} is invite only", pollId));
                }

                Guid newTokenGuid = Guid.NewGuid();

                if (poll.Ballots == null)
                {
                    poll.Ballots = new List<Ballot>();
                }

                Ballot newBallot = new Ballot { TokenGuid = newTokenGuid, ManageGuid = Guid.NewGuid() };
                poll.Ballots.Add(newBallot);

                _metricHandler.HandleBallotAddedEvent(newBallot, pollId);

                context.SaveChanges();

                return newTokenGuid;
            }
        }

        #endregion

    }
}