
using VotingApplication.Data.Context;

namespace VotingApplication.Web.Tests.E2E.Helpers
{
    public interface ITestVotingContext : IVotingContext
    {
        void ReloadEntity(object entity);
    }
}
