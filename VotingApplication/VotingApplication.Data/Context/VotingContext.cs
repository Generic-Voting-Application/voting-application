using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using VotingApplication.Data.Model;

namespace VotingApplication.Data.Context
{
    public class VotingContext : DbContext, IVotingContext
    {
        public VotingContext()
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;
            this.Configuration.UseDatabaseNullSemantics = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new VoteConfiguration());
        }

        public IDbSet<Option> Options { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<Vote> Votes { get; set; }
        public IDbSet<Session> Sessions { get; set; }
        public IDbSet<OptionSet> OptionSets { get; set; }
    }

    public class VoteConfiguration : EntityTypeConfiguration<Vote>
    {
        public VoteConfiguration()
        {
            this.Property(v => v.OptionId).HasColumnName("OptionId");
            this.Property(v => v.UserId).HasColumnName("UserId");
            this.Property(v => v.SessionId).HasColumnName("SessionId");
        }
    }
}
