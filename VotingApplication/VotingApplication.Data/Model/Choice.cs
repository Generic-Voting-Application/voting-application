namespace VotingApplication.Data.Model
{
    public class Choice
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int PollChoiceNumber { get; set; }
    }
}
