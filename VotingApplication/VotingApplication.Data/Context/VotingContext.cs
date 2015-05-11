using System.Data.Entity;
using VotingApplication.Data.Model;

namespace VotingApplication.Data.Context
{
    public class VotingContext : DbContext, IVotingContext
    {
        public VotingContext()
            : base("VotingContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;
            this.Configuration.UseDatabaseNullSemantics = true;
        }

        public IDbSet<Option> Options { get; set; }
        public IDbSet<Vote> Votes { get; set; }
        public IDbSet<Poll> Polls { get; set; }
        public IDbSet<Ballot> Ballots { get; set; }
        public IDbSet<Metric> Metrics { get; set; }
    }
}
