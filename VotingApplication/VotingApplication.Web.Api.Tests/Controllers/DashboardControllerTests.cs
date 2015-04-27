using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Tests.TestHelpers;

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
                var existingPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true);

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);


                List<DashboardPollResponseModel> response = controller.Polls();


                CollectionAssert.AreEquivalent(new List<Poll>(), response);
            }

            [TestMethod]
            public void UserWithPolls_ReturnsAllPolls()
            {
                var existingPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
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
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NoCopyPollRequest_ThrowsBadRequestException()
            {
                var existingPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true);

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);


                controller.Copy(null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void UnknownPollToCopy_ThrowsBadRequestException()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");
                Guid unknownPollId = new Guid("F5F5AF58-4190-4275-8178-FED76105F6BB");

                var existingPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
                {
                    new Poll() { UUID = pollId }
                };

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);

                var request = new CopyPollRequestModel() { UUIDToCopy = unknownPollId };

                controller.Copy(request);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
            public void PollDoesNotBelongToUser_ThrowsForbiddenException()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");

                var existingPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
                {
                    new Poll() { CreatorIdentity = UserId1, UUID = pollId }
                };

                IContextFactory mockContextFactory = CreateContextFactory(existingPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId2);

                var request = new CopyPollRequestModel() { UUIDToCopy = pollId };

                controller.Copy(request);
            }

            [TestMethod]
            public void AddsNewPoll()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");

                var existingPoll = new Poll() { CreatorIdentity = UserId1, UUID = pollId };

                var dbPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
                {
                    existingPoll
                };

                IContextFactory mockContextFactory = CreateContextFactory(dbPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);

                var request = new CopyPollRequestModel() { UUIDToCopy = pollId };


                controller.Copy(request);

                Assert.AreEqual(2, dbPolls.Count());
            }

            [TestMethod]
            public void NewPollNameIsOldPollNamePrependedWithCopyOf()
            {
                const string pollName = "Where shall we go today?";
                const string copiedPollName = "Copy of Where shall we go today?";

                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");

                var existingPoll = new Poll() { CreatorIdentity = UserId1, UUID = pollId, Name = pollName };

                var dbPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
                {
                    existingPoll
                };

                IContextFactory mockContextFactory = CreateContextFactory(dbPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);

                var request = new CopyPollRequestModel() { UUIDToCopy = pollId };


                controller.Copy(request);

                List<Poll> polls = dbPolls.ToList();
                Poll copiedPoll = polls[1];


                Assert.AreEqual(copiedPollName, copiedPoll.Name);
            }

            [TestMethod]
            public void CopiedPollHasSameValuesAsOriginal()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");
                const string creator = "Someone";
                const PollType pollType = PollType.UpDown;
                const int maxPoints = 9;
                const int maxPerVote = 2;
                const bool inviteOnly = true;
                const bool namedVoting = true;
                DateTimeOffset? expiryDate = new DateTimeOffset(2015, 6, 2, 14, 2, 56, new TimeSpan());
                const bool optionAdding = true;

                var existingPoll = new Poll()
                {
                    UUID = pollId,
                    Creator = creator,
                    CreatorIdentity = UserId1,
                    PollType = pollType,
                    MaxPoints = maxPoints,
                    MaxPerVote = maxPerVote,
                    InviteOnly = inviteOnly,
                    NamedVoting = namedVoting,
                    ExpiryDate = expiryDate,
                    OptionAdding = optionAdding
                };

                var dbPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
                {
                    existingPoll
                };

                IContextFactory mockContextFactory = CreateContextFactory(dbPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);

                var request = new CopyPollRequestModel() { UUIDToCopy = pollId };


                controller.Copy(request);

                List<Poll> polls = dbPolls.ToList();
                Poll copiedPoll = polls[1];


                Assert.AreEqual(creator, copiedPoll.Creator);
                Assert.AreEqual(UserId1, copiedPoll.CreatorIdentity);
                Assert.AreEqual(pollType, copiedPoll.PollType);
                Assert.AreEqual(maxPoints, copiedPoll.MaxPoints);
                Assert.AreEqual(maxPerVote, copiedPoll.MaxPerVote);
                Assert.AreEqual(inviteOnly, copiedPoll.InviteOnly);
                Assert.AreEqual(namedVoting, copiedPoll.NamedVoting);
                Assert.AreEqual(expiryDate, copiedPoll.ExpiryDate);
                Assert.AreEqual(optionAdding, copiedPoll.OptionAdding);
            }

            [TestMethod]
            public void CopiedPollHasSameOptionsAsOriginal()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");

                var options = new List<Option>()
                {
                    new Option()
                    {
                        Name = "One",
                        Description = "Option 1"
                    },
                    new Option()
                    {
                        Name = "Two",
                        Description = "Option 2"
                    },
                    new Option()
                    {
                        Name = "Three",
                        Description = "Option 3"
                    }
                };

                var existingPoll = new Poll()
                {
                    UUID = pollId,
                    CreatorIdentity = UserId1,
                    Options = options
                };

                var dbPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
                {
                    existingPoll
                };

                IContextFactory mockContextFactory = CreateContextFactory(dbPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);

                var request = new CopyPollRequestModel() { UUIDToCopy = pollId };


                controller.Copy(request);

                List<Poll> polls = dbPolls.ToList();
                List<Option> originalPollOptions = polls[0].Options;
                List<Option> copiedPollOptions = polls[1].Options;


                Assert.AreEqual(originalPollOptions[0].Name, copiedPollOptions[0].Name);
                Assert.AreEqual(originalPollOptions[0].Description, copiedPollOptions[0].Description);

                Assert.AreEqual(originalPollOptions[1].Name, copiedPollOptions[1].Name);
                Assert.AreEqual(originalPollOptions[1].Description, copiedPollOptions[1].Description);

                Assert.AreEqual(originalPollOptions[2].Name, copiedPollOptions[2].Name);
                Assert.AreEqual(originalPollOptions[2].Description, copiedPollOptions[2].Description);
            }

            [TestMethod]
            public void ResponseContainsNewUUIDAndManageId()
            {
                Guid pollId = new Guid("00DB2F1B-C4F5-44D3-960C-386CEB9690C4");

                var existingPoll = new Poll() { CreatorIdentity = UserId1, UUID = pollId };

                var dbPolls = new InMemoryDbSet<Poll>(clearDownExistingData: true)
                {
                    existingPoll
                };

                IContextFactory mockContextFactory = CreateContextFactory(dbPolls);
                DashboardController controller = CreateDashboardController(mockContextFactory);

                controller.User = CreateAuthenticatedUser(UserId1);

                var request = new CopyPollRequestModel() { UUIDToCopy = pollId };


                CopyPollResponseModel response = controller.Copy(request);

                Poll copiedPoll = dbPolls.ToList()[1];

                Assert.AreEqual(copiedPoll.UUID, response.newPollId);
                Assert.AreEqual(copiedPoll.ManageId, response.newManageId);
            }

            [TestMethod]
            public void CopyPollGeneratesAMetric()
            {
                // Arrange
                Poll existingPoll = new Poll() { CreatorIdentity = UserId1, UUID = Guid.NewGuid(), Name = "Existing Poll" };
                var polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(existingPoll);

                IContextFactory contextFactory = CreateContextFactory(polls);
                var metricHandler = new Mock<IMetricHandler>();
                DashboardController controller = CreateDashboardController(contextFactory, metricHandler.Object);
                controller.User = CreateAuthenticatedUser(UserId1);

                // Act
                controller.Copy(new CopyPollRequestModel() { UUIDToCopy = existingPoll.UUID });

                // Assert
                metricHandler.Verify(m => m.PollClonedEvent(It.Is<Poll>(p => p.Name == "Copy of Existing Poll" && p.UUID != Guid.Empty)), Times.Once());
            }
        }

        private static IContextFactory CreateContextFactory(IDbSet<Poll> polls)
        {
            var mockContext = new Mock<IVotingContext>();
            mockContext.Setup(c => c.Polls)
                .Returns(polls);

            var mockContextFactory = new Mock<IContextFactory>();
            mockContextFactory
                .Setup(a => a.CreateContext())
                .Returns(mockContext.Object);

            return mockContextFactory.Object;
        }

        public DashboardController CreateDashboardController(IContextFactory contextFactory)
        {
            var metricHandler = new Mock<IMetricHandler>();
            return CreateDashboardController(contextFactory, metricHandler.Object);
        }

        public DashboardController CreateDashboardController(IContextFactory contextFactory, IMetricHandler metricHandler)
        {
            return new DashboardController(contextFactory, metricHandler)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        public IPrincipal CreateAuthenticatedUser(string userId)
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
