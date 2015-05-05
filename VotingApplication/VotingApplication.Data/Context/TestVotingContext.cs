using System.Data.Entity;
using VotingApplication.Data.Model;

namespace VotingApplication.Data.Context
{
    public class TestVotingContext : DbContext, ITestVotingContext
    {
        public TestVotingContext()
            : base("TestVotingContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;
            this.Configuration.UseDatabaseNullSemantics = true;

            Database.SetInitializer<TestVotingContext>(new CreateDatabaseIfNotExists<TestVotingContext>());
        }

        public IDbSet<Option> Options { get; set; }
        public IDbSet<Vote> Votes { get; set; }
        public IDbSet<Poll> Polls { get; set; }
        public IDbSet<Ballot> Ballots { get; set; }

        public void ReloadEntity(object entity)
        {
            Entry(entity).Reload();
        }
    }
}
