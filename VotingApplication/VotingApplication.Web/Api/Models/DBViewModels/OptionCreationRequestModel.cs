using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class OptionCreationRequestModel
    {
        [Required]
        [MinLength(1)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
    }
}