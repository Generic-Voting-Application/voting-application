using System.Collections.Generic;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ResultsRequestResponseModel
    {
        public List<string> Winners { get; set; }
        public List<ResultModel> Results { get; set; }
    }
}