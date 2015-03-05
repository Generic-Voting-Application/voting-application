using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Controllers.API_Controllers
{
    public class ManageVoteController : WebApiController
    {
        public ManageVoteController() : base() { }
        public ManageVoteController(IContextFactory contextFactory) : base(contextFactory) { }

        private VoteRequestResponseModel VoteToModel(Vote vote)
        {
            VoteRequestResponseModel model = new VoteRequestResponseModel();

            if (vote.Option != null)
            {
                model.OptionId = vote.Option.Id;
                model.OptionName = vote.Option.Name;
            }

            model.VoterName = vote.VoterName;
            model.VoteValue = vote.VoteValue;

            return model;
        }

        #region GET

        public List<VoteRequestResponseModel> Get(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll poll = context.Polls.Where(s => s.ManageId == manageId).Include(s => s.Options).SingleOrDefault();

                if (poll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                List<Vote> votes = context.Votes.Where(v => v.PollId == poll.UUID).ToList();

                return votes.Select(VoteToModel).ToList();
            }
        }

        #endregion

        #region DELETE

        public void Delete(Guid manageId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.ManageId == manageId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                List<Vote> votesInManagedPoll = context.Votes.Where(v => v.PollId == matchingPoll.UUID).ToList();
                foreach (Vote vote in votesInManagedPoll)
                {
                    context.Votes.Remove(vote);
                }

                context.SaveChanges();

                return;
            }
        }

        public void Delete(Guid manageId, long voteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                Poll matchingPoll = context.Polls.Where(s => s.ManageId == manageId).FirstOrDefault();
                if (matchingPoll == null)
                {
                    this.ThrowError(HttpStatusCode.NotFound, string.Format("Poll {0} not found", manageId));
                }

                Vote matchingVote = context.Votes.Where(v => v.Id == voteId && v.PollId == matchingPoll.UUID).FirstOrDefault();

                context.Votes.Remove(matchingVote);
                context.SaveChanges();

                return;
            }
        }

        #endregion
    }
}