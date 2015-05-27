using System.Data.Entity;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Tests.E2E.Helpers
{
    public class TestVotingContext : DbContext, ITestVotingContext
    {
        public TestVotingContext()
            : base("TestVotingContext")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            Configuration.UseDatabaseNullSemantics = true;

            Database.SetInitializer(new CreateDatabaseIfNotExists<TestVotingContext>());
        }

        public IDbSet<Choice> Choices { get; set; }
        public IDbSet<Vote> Votes { get; set; }
        public IDbSet<Poll> Polls { get; set; }
        public IDbSet<Ballot> Ballots { get; set; }
        public IDbSet<Metric> Metrics { get; set; }

        public void ReloadEntity(object entity)
        {
            Entry(entity).Reload();
        }

        public void ClearDatabase()
        {
            ((DbSet<Choice>)Choices).RemoveRange(Choices);
            ((DbSet<Vote>)Votes).RemoveRange(Votes);
            ((DbSet<Poll>)Polls).RemoveRange(Polls);
            ((DbSet<Ballot>)Ballots).RemoveRange(Ballots);
            ((DbSet<Metric>)Metrics).RemoveRange(Metrics);

            SaveChanges();
        }
    }
}
