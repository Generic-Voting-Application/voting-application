using System;

namespace VotingApplication.Data.Model
{
    public class Ballot
    {
        public long Id { get; set; }
        public Guid TokenGuid { get; set; }
        public string Email { get; set; }
        public String VoterName { get; set; }
    }
}
