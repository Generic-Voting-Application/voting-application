namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class VoteRequestResponseModel
    {
        public long VoterId { get; set; }
        public string VoterName { get; set; }
        public long OptionId { get; set; }
        public string OptionName { get; set; }
        public int VoteValue { get; set; }
    }
}