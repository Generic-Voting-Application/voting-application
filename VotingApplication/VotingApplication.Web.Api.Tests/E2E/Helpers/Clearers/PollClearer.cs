using System.Data.Entity;
using System.Linq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers
{
    public class PollClearer : Clearer
    {
        public PollClearer(IVotingContext context) : base(context) { }

        public void ClearPoll(Poll poll)
        {
            Poll dbPoll = _context.Polls
                                    .Where(p => p.UUID == poll.UUID)
                                    .Include(p => p.Options)
                                    .Include(p => p.Ballots)
                                    .Include(p => p.Ballots.Select(b => b.Votes))
                                    .Single<Poll>();

            ((DbSet<Vote>)_context.Votes).RemoveRange(dbPoll.Ballots.SelectMany(b => b.Votes).ToList());
            ((DbSet<Option>)_context.Options).RemoveRange(dbPoll.Options);
            ((DbSet<Ballot>)_context.Ballots).RemoveRange(dbPoll.Ballots);
            _context.Polls.Remove(poll);

            _context.SaveChanges();
        }
    }
}
