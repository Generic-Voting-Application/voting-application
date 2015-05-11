using FakeDbSet;
using System.Data.Entity;

namespace VotingApplication.Web.Tests.TestHelpers
{
    public static class DbSetTestHelper
    {
        public static IDbSet<T> CreateMockDbSet<T>() where T : class
        {
            return new InMemoryDbSet<T>(clearDownExistingData: true);
        }
    }
}
