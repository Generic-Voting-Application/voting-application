using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollMiscRequest
    {
        [Required]
        public bool InviteOnly { get; set; }
        [Required]
        public bool NamedVoting { get; set; }
        [Required]
        public bool ChoiceAdding { get; set; }
        [Required]
        public bool IsElectionMode { get; set; }
    }
}