using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class BallotRequestModel
    {
        public BallotRequestModel()
        {
            Votes = new List<VoteRequestModel>();
        }

        public string VoterName { get; set; }
        public List<VoteRequestModel> Votes { get; set; }
    }

    public class VoteRequestModel
    {
        [Required]
        public long ChoiceId { get; set; }
        public int VoteValue { get; set; }
    }
}