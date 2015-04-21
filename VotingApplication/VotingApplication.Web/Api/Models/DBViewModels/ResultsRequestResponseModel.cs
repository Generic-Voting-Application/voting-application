using System.Collections.Generic;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ResultsRequestResponseModel
    {
        public List<Option> Winners;
        public List<ResultModel> Results;
        public List<VoteRequestResponseModel> Votes;
    }
}