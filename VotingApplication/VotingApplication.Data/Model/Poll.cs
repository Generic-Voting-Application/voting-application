using System;
using System.Collections.Generic;

namespace VotingApplication.Data.Model
{
    public class Poll
    {
        public long Id { get; set; }

        public Guid UUID { get; set; }
        public Guid ManageId { get; set; }
        public string Name { get; set; }

        public string Creator { get; set; }

        public PollType PollType { get; set; }

        public string CreatorIdentity { get; set; }

        public List<Option> Options { get; set; }

        public int MaxPoints { get; set; }
        public int MaxPerVote { get; set; }

        public bool InviteOnly { get; set; }
        public List<Token> Tokens { get; set; }

        public bool NamedVoting { get; set; }

        public bool RequireAuth { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }

        public bool OptionAdding { get; set; }

        public DateTime LastUpdated { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
