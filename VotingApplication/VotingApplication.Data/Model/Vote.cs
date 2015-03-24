using System;
namespace VotingApplication.Data.Model
{
    public class Vote
    {
        public long Id { get; set; }

        public Option Option { get; set; }

        public int VoteValue { get; set; }
        public String VoterName { get; set; }

        public Poll Poll { get; set; }
        public Token Token { get; set; }
    }
}
