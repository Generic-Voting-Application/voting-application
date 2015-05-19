using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Validators
{
    public class RankedVoteValidator : IVoteValidator
    {
        public void Validate(List<VoteRequestModel> voteRequests, Poll poll, ModelStateDictionary modelState)
        {
            if (voteRequests.GroupBy(v => v.ChoiceId).Any(g => g.Count() > 1))
            {
                modelState.AddModelError("Vote", "Invalid number of votes");
            }

            List<VoteRequestModel> sortedVotes = voteRequests.OrderBy(v => v.VoteValue).ToList();

            // Order the vote values and check that they are 1..n 
            if (!voteRequests.OrderBy(v => v.VoteValue).Select(v => v.VoteValue).SequenceEqual(Enumerable.Range(1, voteRequests.Count)))
            {
                modelState.AddModelError("VoteValue", String.Format("Invalid vote value, values should be greater than 0 and contain values from 1 to n where n is the number of votes"));
            }
        }
    }
}