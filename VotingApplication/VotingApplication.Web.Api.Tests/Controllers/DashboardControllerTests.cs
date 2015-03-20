using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        const string UserId1 = "4AEAE121-D540-48BF-907A-AA454248C0C0";
        const string UserId2 = "C9FB9965-98CD-4D1A-B530-10E8E8D26AFB";


        [TestClass]
        public class PollsTests : DashboardControllerTests
        {
            [TestMethod]
            public void UserWithNoPolls_ReturnsEmptyPollList()
            {
                var existingPolls = Enumerable.Empty<Poll>();

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);


                List<DashboardPollResponseModel> response = controller.Polls();


                CollectionAssert.AreEquivalent(new List<Poll>(), response);
            }

            [TestMethod]
            public void UserWithPolls_ReturnsAllPolls()
            {
                var existingPolls = new List<Poll>
                {
                    new Poll() { CreatorIdentity = UserId1 },
                    new Poll() { CreatorIdentity = UserId1 },
                    new Poll() { CreatorIdentity = UserId1 }
                };

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);


                List<DashboardPollResponseModel> response = controller.Polls();


                Assert.AreEqual(3, response.Count);
            }
        }

        [TestClass]
        public class CopyTests : DashboardControllerTests
        {
            [TestMethod]
            public void NoCopyPollRequest_ThrowsBadRequestException()
            {
                var existingPolls = Enumerable.Empty<Poll>();

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);


                try
                {
                    controller.Copy(null);
                }
                catch (HttpResponseException exception)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
                }
            }

            [TestMethod]
            public void UnknownPollToCopy_ThrowsBadRequestException()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");
                Guid unknownPollId = new Guid("F5F5AF58-4190-4275-8178-FED76105F6BB");

                var existingPolls = new List<Poll>
                {
                    new Poll() { UUID = pollId }
                };

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);

                var request = new CopyPollRequestModel() { UUIDToCopy = unknownPollId };


                try
                {
                    controller.Copy(request);
                }
                catch (HttpResponseException exception)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
                }
            }

            [TestMethod]
            public void PollDoesNotBelongToUser_ThrowsForbiddenException()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");

                var existingPolls = new List<Poll>()
                {
                    new Poll() { CreatorIdentity = UserId1, UUID = pollId }
                };

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId2);

                var request = new CopyPollRequestModel() { UUIDToCopy = pollId };


                try
                {
                    controller.Copy(request);
                }
                catch (HttpResponseException exception)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, exception.Response.StatusCode);
                }
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
