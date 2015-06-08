using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Controllers
{
    public class ResultsRequestModel
    {
        [Required]
        public Guid PollId { get; set; }

        public Guid BallotGuid { get; set; }
    }
}
