using System;
using System.Data.Entity;
using System.Linq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers
{
    public class VoteClearer : Clearer
    {
        public VoteClearer(ITestVotingContext context) : base(context) { }

        public void ClearLast()
        {
            Vote lastVote = _context.Votes.AsEnumerable().LastOrDefault();
            if (lastVote != null)
            {
                _context.Votes.Remove(lastVote);
                _context.SaveChanges();
            }
        }

        public void ClearLast(int count)
        {
            ((DbSet<Vote>)_context.Votes).RemoveRange(_context.Votes.Skip(Math.Max(0, _context.Votes.Count() - count)).ToList());
            _context.SaveChanges();
        }
    }
}
