namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class VoteRequestResponseModel
    {
        public long VoterId { get; set; }
        public string VoterName { get; set; }
        public long ChoiceId { get; set; }
        public string ChoiceName { get; set; }
        public int VoteValue { get; set; }
        public bool UserHasVoted { get; set; }

        public VoteRequestResponseModel()
        {
            VoterName = "Anonymous User";
        }
    }
}