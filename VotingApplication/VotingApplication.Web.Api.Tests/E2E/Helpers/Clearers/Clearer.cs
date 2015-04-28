using VotingApplication.Data.Context;

namespace VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers
{
    public abstract class Clearer
    {
        protected IVotingContext _context;

        public Clearer(IVotingContext context)
        {
            _context = context;
        }
    }
}
