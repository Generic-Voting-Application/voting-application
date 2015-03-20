using VotingApplication.Data.Model;
namespace VotingApplication.Web.Api.Validators
{
    public interface IVoteValidatorFactory
    {
        IVoteValidator CreateValidator(PollType voteType);
    }
}