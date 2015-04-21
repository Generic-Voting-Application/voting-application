using System;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Validators
{
    public class BasicVoteValidator : IVoteValidator
    {
        public void Validate(List<VoteRequestModel> voteRequests, Poll poll, ModelStateDictionary modelState)
        {
            if (voteRequests.Count > 1)
            {
                modelState.AddModelError("Vote", "Invalid number of votes");
            }
            else
            {
                if (voteRequests.Count == 1 && voteRequests[0].VoteValue != 1)
                {
                    modelState.AddModelError("VoteValue", String.Format("Invalid vote value: {0}", voteRequests[0].VoteValue));
                }
            }
        }
    }
}