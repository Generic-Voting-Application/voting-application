using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class VoteRequestModel
    {
        [Required]
        public long OptionId { get; set; }

        [Required]
        public Guid TokenGuid { get; set; }

        [Range(0, int.MaxValue)]
        public int VoteValue { get; set; }
    }
}