using System;
using System.Collections.Generic;
using System.Data.Entity;
using VotingApplication.Data.Model;

namespace VotingApplication.Data.Context
{
    public interface IVotingContext : IDisposable
    {
        IEnumerable<Option> Options { get; set; }
        IEnumerable<User> Users { get; set; }
        IEnumerable<Vote> Votes { get; set; }
    }
}
