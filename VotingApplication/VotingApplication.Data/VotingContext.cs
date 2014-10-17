using System.Data.Entity;
using VotingApplication.Data.Model;

namespace VotingApplication.Data
{
    public class VotingContext
    {
        public DbSet<Option> Options { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vote> Votes { get; set; }
    }
}
