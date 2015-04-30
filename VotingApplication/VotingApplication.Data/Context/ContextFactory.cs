
namespace VotingApplication.Data.Context
{
    public class ContextFactory : IContextFactory
    {
        public IVotingContext CreateContext()
        {
            return new VotingContext();
        }
    }
}
