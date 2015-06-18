using System.Collections.Generic;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ResultsRequestResponseModel
    {
        public List<Choice> Winners;
        public List<ResultModel> Results;
    }
}