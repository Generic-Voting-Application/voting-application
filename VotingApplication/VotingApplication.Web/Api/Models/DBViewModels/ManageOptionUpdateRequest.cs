using System.Collections.Generic;

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
        public string Name { get; set; }
        public string Description { get; set; }
        public int? PollOptionNumber { get; set; }
    }
}
