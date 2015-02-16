using VotingApplication.Data.Model;
namespace VotingApplication.Web.Api.Validators
{
    public class VoteValidatorFactory : IVoteValidatorFactory
    {
        public IVoteValidator CreateValidator(PollType voteType)
        {
            switch(voteType)
            {
                case PollType.Basic:
                    return new BasicVoteValidator();

                case PollType.Points:
                    return new PointsVoteValidator();

                case PollType.Ranked:
                    return new RankedVoteValidator();

                default:
                    return new BasicVoteValidator();
            }
        }
    }
}
