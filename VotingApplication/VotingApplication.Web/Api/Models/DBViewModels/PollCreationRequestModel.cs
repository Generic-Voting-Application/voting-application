using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class PollCreationRequestModel
    {
        [Required]
        public string PollName { get; set; }
    }
}