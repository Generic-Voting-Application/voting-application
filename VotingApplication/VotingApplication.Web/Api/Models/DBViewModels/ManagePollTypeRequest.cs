
using System.ComponentModel.DataAnnotations;
namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollTypeRequest
    {
        [Required]
        public string PollType { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaxPoints { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaxPerVote { get; set; }
    }
}