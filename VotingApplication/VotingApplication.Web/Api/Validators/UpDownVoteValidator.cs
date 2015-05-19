using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Validators
{
    public class UpDownVoteValidator : IVoteValidator
    {
        public void Validate(List<VoteRequestModel> voteRequests, Poll poll, ModelStateDictionary modelState)
        {
            if (voteRequests.GroupBy(v => v.ChoiceId).Any(g => g.Count() > 1))
            {
                modelState.AddModelError("Vote", "Invalid number of votes");
            }

            if (voteRequests.Any(v => v.VoteValue != Math.Sign(v.VoteValue)))
            {
                modelState.AddModelError("VoteValue", "Invalid vote value, vote values must be 1, -1 or 0");
            }
        }
    }
}