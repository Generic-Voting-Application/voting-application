using System;
using System.Collections.Generic;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class ManagePollRequestResponseModel
    {
        public Guid UUID { get; set; }
        public List<Choice> Choices { get; set; }
        public int InviteeCount { get; set; }
        public int VotersCount { get; set; }
        public string PollType { get; set; }
        public int? MaxPoints { get; set; }
        public int? MaxPerVote { get; set; }
        public string Name { get; set; }
        public bool InviteOnly { get; set; }
        public bool NamedVoting { get; set; }
        public DateTime? ExpiryDateUtc { get; set; }
        public bool ChoiceAdding { get; set; }
        public bool ElectionMode { get; set; }
    }
}