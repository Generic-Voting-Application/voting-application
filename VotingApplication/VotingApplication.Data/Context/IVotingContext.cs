using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VotingApplication.Data.Model;

namespace VotingApplication.Data.Context
{
    public interface IVotingContext : IDisposable
    {
        IDbSet<Option> Options { get; set; }
        IDbSet<User> Users { get; set; }
        IDbSet<Vote> Votes { get; set; }
        IDbSet<Session> Sessions { get; set; }
        IDbSet<Template> Templates { get; set; }

        int SaveChanges();
    }
}
