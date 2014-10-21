using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Data.Context
{
    public interface IContextFactory
    {
        IVotingContext CreateContext();
    }
}
