namespace VotingApplication.Data.Model
{
    public class Vote
    {
        public long Id { get; set; }
        public Choice Choice { get; set; }
        public int VoteValue { get; set; }
        public Ballot Ballot { get; set; }

        public Poll Poll { get; set; }
    }
}
