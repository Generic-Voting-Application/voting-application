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
            if (voteRequests.GroupBy(v => v.ChoiceId).Any(g => g.Count() > 1))
            {
                modelState.AddModelError("Vote", "Invalid number of votes");
            }

            if (voteRequests.Any(v => v.VoteValue > poll.MaxPerVote || v.VoteValue <= 0) || voteRequests.Sum(v => v.VoteValue) > poll.MaxPoints)
            {
                modelState.AddModelError("VoteValue", String.Format("Invalid vote value, vote values must be between 1 and {0} with their total not exceeding {1}", 
                                                                    poll.MaxPerVote, poll.MaxPoints));
            }
        }
    }
}