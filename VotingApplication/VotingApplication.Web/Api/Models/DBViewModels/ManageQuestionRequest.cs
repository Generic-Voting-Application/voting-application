using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManageQuestionRequest
    {
        [Required]
        public string Question { get; set; }
    }
}