using System;
using System.Collections.Generic;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class PollRequestResponseModel
    {
        public string Name { get; set; }
        public string PollType { get; set; }
        public DateTime? ExpiryDateUtc { get; set; }

        public int? MaxPoints { get; set; }
        public int? MaxPerVote { get; set; }

        public Guid TokenGuid { get; set; }

        public List<Choice> Choices { get; set; }

        public bool NamedVoting { get; set; }
        public bool ChoiceAdding { get; set; }
        public bool ElectionMode { get; set; }

        public bool UserHasVoted { get; set; }
    }
}