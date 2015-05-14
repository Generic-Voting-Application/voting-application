
namespace VotingApplication.Data.Context
{
    public interface ITestVotingContext : IVotingContext
    {
        void ReloadEntity(object entity);
    }
}
