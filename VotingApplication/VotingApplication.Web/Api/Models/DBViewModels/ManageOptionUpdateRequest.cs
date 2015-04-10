using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManageOptionUpdateRequest
    {
        public ManageOptionUpdateRequest()
        {
            Options = new List<OptionUpdate>();
        }

        public List<OptionUpdate> Options { get; set; }
    }

    public class OptionUpdate
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public int? OptionNumber { get; set; }
    }
}
