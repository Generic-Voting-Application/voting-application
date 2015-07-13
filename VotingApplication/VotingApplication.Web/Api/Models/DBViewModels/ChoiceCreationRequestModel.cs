using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ChoiceCreationRequestModel
    {
        [Required]
        [MinLength(1)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}