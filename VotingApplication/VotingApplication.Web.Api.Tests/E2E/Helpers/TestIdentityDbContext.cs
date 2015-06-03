using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using VotingApplication.Web.Api.Models;

namespace VotingApplication.Web.Api.Tests.E2E.Helpers
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
