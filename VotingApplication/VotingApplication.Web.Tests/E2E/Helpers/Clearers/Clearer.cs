namespace VotingApplication.Web.Tests.E2E.Helpers.Clearers
{
    public abstract class Clearer
    {
        protected ITestVotingContext _context;

        public Clearer(ITestVotingContext context)
        {
            _context = context;
        }
    }
}
