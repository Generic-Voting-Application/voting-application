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

        [Range(1, int.MaxValue)]
        public int MaxPoints { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxPerVote { get; set; }

        public long TemplateId { get; set; }
        public bool InviteOnly { get; set; }
        public bool NamedVoting { get; set; }
        public bool RequireAuth { get; set; }
        public bool Expires { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }

        public bool OptionAdding { get; set; }
    }
}