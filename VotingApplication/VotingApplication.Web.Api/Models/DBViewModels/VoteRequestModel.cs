using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class VoteRequestModel
    {
        [Required]
        public long OptionId { get; set; }

        public int VoteValue { get; set; }

        public string VoterName { get; set; }
    }
}