using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class PollCreationRequestModel
    {
        [Required]
        public string PollName { get; set; }

        public List<Choice> Choices { get; set; }
    }
}