using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using VotingApplication.Web.Api.Models;

namespace VotingApplication.Web.Tests.E2E.Helpers
{
    public class TestIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public TestIdentityDbContext()
            : base("TestVotingContext", throwIfV1Schema: false)
        {
        }

        public void ClearUsers()
        {
            ((DbSet<ApplicationUser>)Users).RemoveRange(Users);

            SaveChanges();
        }
    }
}
