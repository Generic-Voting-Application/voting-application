using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Data.Model
{
    public class Poll
    {
        public long Id { get; set; }

        public Guid UUID { get; set; }
        public Guid ManageID { get; set; }
        public string Name { get; set; }

        public string Creator { get; set; }

        public String VotingStrategy { get; set; }

        [NotMapped]
        public string Email { get; set; }
        [NotMapped]
        public List<string> Invites { get; set; }

        public string CreatorIdentity { get; set; }

        public List<Option> Options { get; set; }

        public int MaxPoints { get; set; }
        public int MaxPerVote { get; set; }

        public bool InviteOnly { get; set; }
        public List<Token> Tokens { get; set; }

        public List<ChatMessage> ChatMessages { get; set; }

        public bool AnonymousVoting { get; set; }

        public bool RequireAuth { get; set; }
        public bool Expires { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }

        public bool OptionAdding { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
