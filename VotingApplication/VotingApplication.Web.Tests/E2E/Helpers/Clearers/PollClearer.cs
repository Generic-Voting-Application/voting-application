using System.Data.Entity;
using System.Linq;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Tests.E2E.Helpers.Clearers
{
    public class PollClearer : Clearer
    {
        public PollClearer(ITestVotingContext context) : base(context) { }

        public void ClearPoll(Poll poll)
        {
            Poll dbPoll = _context.Polls
                                    .Where(p => p.UUID == poll.UUID)
                                    .Include(p => p.Choices)
                                    .Include(p => p.Ballots)
                                    .Include(p => p.Ballots.Select(b => b.Votes))
                                    .Single<Poll>();

            ((DbSet<Vote>)_context.Votes).RemoveRange(dbPoll.Ballots.SelectMany(b => b.Votes).ToList());
            ((DbSet<Choice>)_context.Choices).RemoveRange(dbPoll.Choices);
            ((DbSet<Ballot>)_context.Ballots).RemoveRange(dbPoll.Ballots);
            _context.Polls.Remove(dbPoll);

            _context.SaveChanges();
        }
    }
}
