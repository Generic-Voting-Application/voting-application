namespace VotingApplication.Web.Api.Models.DBViewModels
{
    public class VoteRequestResponseModel
    {
        public string VoterName { get; set; }
        public long OptionId { get; set; }
        public string OptionName { get; set; }
        public int VoteValue { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
    }
}