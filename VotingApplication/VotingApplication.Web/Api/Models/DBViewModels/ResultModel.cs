using System.Collections.Generic;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ResultModel
    {
        public Choice Choice;
        public int Sum;
        public List<ResultVoteModel> Voters;
    }
}