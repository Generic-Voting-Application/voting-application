namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class PollRequestChoiceResponseModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int PollChoiceNumber { get; set; }

        public int VoteValue { get; set; }
    }
}