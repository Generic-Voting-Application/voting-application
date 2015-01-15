using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class OptionRequestResponseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
    }
}