
namespace VotingApplication.Web.Api.Controllers
{
    public class ManagePollTypeRequestResponse
    {
        public string PollType { get; set; }
        public int? MaxPoints { get; set; }
        public int? MaxPerVote { get; set; }
        public bool PollHasVotes { get; set; }
    }
}
