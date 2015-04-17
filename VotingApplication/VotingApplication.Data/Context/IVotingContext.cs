using System;
using System.Data.Entity;
using VotingApplication.Data.Model;

namespace VotingApplication.Data.Context
{
    public interface IVotingContext : IDisposable
    {
        IDbSet<Option> Options { get; set; }
        IDbSet<Vote> Votes { get; set; }
        IDbSet<Poll> Polls { get; set; }
        IDbSet<Ballot> Ballots { get; set; }
        IDbSet<Event> Events { get; set; }

        int SaveChanges();
    }
}
