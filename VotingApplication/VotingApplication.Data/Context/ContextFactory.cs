using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingApplication.Data.Context
{
    public class ContextFactory : IContextFactory
    {
        public IVotingContext CreateContext()
        {
            return new VotingContext();
        }
    }
}
