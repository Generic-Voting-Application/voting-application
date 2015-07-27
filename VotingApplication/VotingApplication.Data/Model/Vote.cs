using System.ComponentModel.DataAnnotations.Schema;
namespace VotingApplication.Data.Model
{
    public class Vote
    {
        public long Id { get; set; }
        [Index("IX_ChoiceBallot", 0, IsUnique = true)]
        public Choice Choice { get; set; }
        public int VoteValue { get; set; }
        [Index("IX_ChoiceBallot", 1, IsUnique = true)]
        public Ballot Ballot { get; set; }

        public Poll Poll { get; set; }
    }
}
