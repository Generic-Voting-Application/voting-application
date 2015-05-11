using System;
using System.Data.Entity;
using System.Threading.Tasks;
using VotingApplication.Data.Model;

namespace VotingApplication.Data.Context
{
    public interface IVotingContext : IDisposable
    {
        IDbSet<Option> Options { get; set; }
        IDbSet<Vote> Votes { get; set; }
        IDbSet<Poll> Polls { get; set; }
        IDbSet<Ballot> Ballots { get; set; }
        IDbSet<Metric> Metrics { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
