namespace VotingApplication.Data.Model
{
    public class Vote
    {
        public long Id { get; set; }

        public long OptionId { get; set; }
        public Option Option { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public long SessionId { get; set; }
        public User Session { get; set; }
    }
}
