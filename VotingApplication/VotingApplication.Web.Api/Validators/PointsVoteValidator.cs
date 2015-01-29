using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Validators
{
    public class PointsVoteValidator : IVoteValidator
    {
        public void Validate(List<VoteRequestModel> voteRequests, Poll poll, ModelStateDictionary modelState)
        {
            if (voteRequests.GroupBy(v => v.OptionId).Any(g => g.Count() > 1))
            {
                modelState.AddModelError("Vote", "Invalid number of votes");
            }

            foreach (VoteRequestModel voteRequest in voteRequests)
            {
                if (voteRequest.VoteValue > poll.MaxPerVote || voteRequest.VoteValue > poll.MaxPoints || voteRequest.VoteValue <= 0)
                {
                    modelState.AddModelError("VoteValue", String.Format("Invalid vote value: {0}", voteRequest.VoteValue));
                }
            }
        }
    }
}