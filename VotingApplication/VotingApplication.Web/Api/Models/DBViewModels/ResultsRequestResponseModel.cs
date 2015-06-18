using System.Collections.Generic;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ResultsRequestResponseModel
    {
        public List<string> Winners;
        public List<ResultModel> Results;
    }
}