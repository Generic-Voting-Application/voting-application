using System.Collections.Generic;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ResultModel
    {
        public string ChoiceName;
        public int ChoiceNumber;
        public int Sum;
        public List<ResultVoteModel> Voters;
    }
}