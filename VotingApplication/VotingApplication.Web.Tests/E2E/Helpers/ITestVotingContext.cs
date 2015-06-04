
using VotingApplication.Data.Context;

namespace VotingApplication.Web.Api.Tests.E2E.Helpers
{
    public interface ITestVotingContext : IVotingContext
    {
        void ReloadEntity(object entity);
    }
}
