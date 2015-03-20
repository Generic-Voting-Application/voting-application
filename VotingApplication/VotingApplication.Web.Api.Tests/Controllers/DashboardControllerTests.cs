using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class DashboardControllerTests
    {
        const string UserId = "4AEAE121-D540-48BF-907A-AA454248C0C0";

        [TestClass]
        public class PollsTests : DashboardControllerTests
        {
            [TestMethod]
            public void UserWithNoPolls_ReturnsEmptyPollList()
            {
                var existingPolls = Enumerable.Empty<Poll>();

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(userId: "some new id that cannot exist");


                var response = controller.Polls();


                CollectionAssert.AreEquivalent(new List<Poll>(), response);
            }

            [TestMethod]
            public void UserWithPolls_ReturnsAllPolls()
            {
                var existingPolls = new List<Poll>
                {
                    Mock.Of<Poll>(p => p.CreatorIdentity == UserId), 
                    Mock.Of<Poll>(p => p.CreatorIdentity == UserId), 
                    Mock.Of<Poll>(p => p.CreatorIdentity == UserId)
                };

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId);


                List<DashboardPollResponseModel> response = controller.Polls();


                Assert.AreEqual(3, response.Count);
            }
        }

        private IContextFactory CreateContextFactory(IEnumerable<Poll> polls)
        {
            var pollsToAdd = new InMemoryDbSet<Poll>(clearDownExistingData: true);
            foreach (Poll poll in polls)
            {
                pollsToAdd.Add(poll);
            }

            var mockContext = new Mock<IVotingContext>();
            mockContext.Setup(c => c.Polls)
                .Returns(pollsToAdd);

            var mockContextFactory = new Mock<IContextFactory>();
            mockContextFactory
                .Setup(a => a.CreateContext())
                .Returns(mockContext.Object);

            return mockContextFactory.Object;
        }

        private DashboardController CreateDashboardController(IContextFactory contextFactory)
        {
            return new DashboardController(contextFactory)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        private IPrincipal CreateAuthenticatedUser(string userId)
        {
            var claim = new Claim("test", userId);
            var mockIdentity = new Mock<ClaimsIdentity>();
            mockIdentity
                .Setup(ci => ci.FindFirst(It.IsAny<string>()))
                .Returns(claim);

            mockIdentity
                .Setup(i => i.IsAuthenticated)
                .Returns(true);

            var principal = new Mock<IPrincipal>();
            principal
                .Setup(ip => ip.Identity)
                .Returns(mockIdentity.Object);

            return principal.Object;
        }
    }
}
