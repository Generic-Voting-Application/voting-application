using System;
using System.ComponentModel.DataAnnotations;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class PollCreationRequestModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Creator { get; set; }

        [Required]
        public string VotingStrategy { get; set; } 

        [EmailAddress]
        public string Email { get; set; }

        public long TemplateId { get; set; }
        public int MaxPoints { get; set; }
        public int MaxPerVote { get; set; }
        public bool InviteOnly { get; set; }
        public bool AnonymousVoting { get; set; }
        public bool RequireAuth { get; set; }
        public bool Expires { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }

        public bool OptionAdding { get; set; }
    }
}