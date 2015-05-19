using System;
using System.Collections.Generic;

namespace VotingApplication.Data.Model
{
    public class Poll
    {
        public Poll()
        {
            Ballots = new List<Ballot>();
            Options = new List<Option>();
        }

        public long Id { get; set; }

        public Guid UUID { get; set; }
        public Guid ManageId { get; set; }
        public string Name { get; set; }

        public string Creator { get; set; }

        public PollType PollType { get; set; }

        public string CreatorIdentity { get; set; }

        public List<Option> Options { get; set; }

        public int? MaxPoints { get; set; }
        public int? MaxPerVote { get; set; }

        public bool InviteOnly { get; set; }
        public List<Ballot> Ballots { get; set; }

        public bool NamedVoting { get; set; }

        public DateTime? ExpiryDateUtc { get; set; }

        public bool OptionAdding { get; set; }
        public bool HiddenResults { get; set; }

        public DateTime LastUpdatedUtc { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}
