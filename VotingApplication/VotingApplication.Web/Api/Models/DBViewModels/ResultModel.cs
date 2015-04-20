using System.Collections.Generic;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ResultModel
    {
        public Option Option;
        public int Sum;
        public List<ResultVoteModel> Voters;
    }
}