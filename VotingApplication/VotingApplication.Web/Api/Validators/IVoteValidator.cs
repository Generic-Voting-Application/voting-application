using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Validators
{
    public interface IVoteValidator
    {
        void Validate(List<VoteRequestModel> voteRequestModel, Poll poll,  ModelStateDictionary modelState);
    }
}