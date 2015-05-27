using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManageChoiceUpdateRequest
    {
        public ManageChoiceUpdateRequest()
        {
            Choices = new List<ChoiceUpdate>();
        }

        public List<ChoiceUpdate> Choices { get; set; }
    }

    public class ChoiceUpdate
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ChoiceNumber { get; set; }
    }
}
