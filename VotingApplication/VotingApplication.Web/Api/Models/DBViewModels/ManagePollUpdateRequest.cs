using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollUpdateRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string VotingStrategy { get; set; }
        [Range(1, int.MaxValue)]
        public int MaxPoints { get; set; }
        [Range(1, int.MaxValue)]
        public int MaxPerVote { get; set; }
        public List<Option> Options { get; set; }
        public List<Token> Voters { get; set; }
        public bool InviteOnly { get; set; }
        public bool NamedVoting { get; set; }
        public bool RequireAuth { get; set; }
        public bool Expires { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public bool OptionAdding { get; set; }
    }
}