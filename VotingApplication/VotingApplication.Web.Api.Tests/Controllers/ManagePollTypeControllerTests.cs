using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
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
    public class ManagePollTypeControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("961efb70-6767-4658-a95d-fea312c802ec");

        [TestClass]
        public class PutTests : ManagePollTypeControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollTypeRequest request = new ManagePollTypeRequest { };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(Guid.NewGuid(), request);
            }

            [TestMethod]
            public void SetPollType_SetsPollType()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "UpDown", MaxPerVote = 1, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(PollType.UpDown, existingPoll.PollType);
            }

            [TestMethod]
            public void SetMaxValues_SetsMaxValues()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();

                int maxPerVote = 15;
                int maxPoints = 17;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "UpDown", MaxPerVote = maxPerVote, MaxPoints = maxPoints };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(maxPerVote, existingPoll.MaxPerVote);
                Assert.AreEqual(maxPoints, existingPoll.MaxPoints);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void InvalidPollType_ReturnsBadRequest()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "NotAnOption", MaxPerVote = 1, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(0, existingVotes.Local.Count);
            }

            [TestMethod]
            public void ChangedPollType_ClearsVotes()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "UpDown", MaxPerVote = 1, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(0, existingVotes.Local.Count);
            }

            [TestMethod]
            public void ChangedPollMaxPoints_ClearsVotes()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);
                existingPoll.PollType = PollType.Points;
                existingPoll.MaxPoints = 1;
                existingPoll.MaxPerVote = 1;

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Points", MaxPerVote = 1, MaxPoints = 2 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(0, existingVotes.Local.Count);
            }

            [TestMethod]
            public void ChangedPollMaxPerVote_ClearsVotes()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);
                existingPoll.PollType = PollType.Points;
                existingPoll.MaxPoints = 1;
                existingPoll.MaxPerVote = 1;

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Points", MaxPerVote = 2, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(0, existingVotes.Local.Count);
            }

            [TestMethod]
            public void ChangedPollMaxPerVoteOnNonPointsPoll_LeavesVotes()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPoll.PollType = PollType.Basic;
                existingPoll.MaxPoints = 1;
                existingPoll.MaxPerVote = 1;
                existingPolls.Add(existingPoll);

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Basic", MaxPerVote = 1, MaxPoints = 2 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(1, existingVotes.Local.Count);
            }

            [TestMethod]
            public void UnChangedPollType_LeavesVotes()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPoll.PollType = PollType.Basic;
                existingPoll.MaxPoints = 1;
                existingPoll.MaxPerVote = 1;
                existingPolls.Add(existingPoll);

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Basic", MaxPerVote = 1, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(1, existingVotes.Local.Count);
            }

            [TestMethod]
            public void ChangesPollTypeGeneratesMetric()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "UpDown", MaxPerVote = 1, MaxPoints = 1 };

                Mock<IMetricEventHandler> mockMetricHandler = new Mock<IMetricEventHandler>();
                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory, mockMetricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                mockMetricHandler.Verify(m => m.PollTypeChangedEvent(PollType.UpDown, 1, 1, existingPoll.UUID), Times.Once());
            }

            [TestMethod]
            public void ChangesPollPointValuesInPointsPollGeneratesMetric()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPoll.PollType = PollType.Points;
                existingPoll.MaxPerVote = 3;
                existingPoll.MaxPoints = 7;
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Points", MaxPerVote = 5, MaxPoints = 8 };

                Mock<IMetricEventHandler> mockMetricHandler = new Mock<IMetricEventHandler>();
                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory, mockMetricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                mockMetricHandler.Verify(m => m.PollTypeChangedEvent(PollType.Points, 5, 8, existingPoll.UUID), Times.Once());
            }

            [TestMethod]
            public void ChangesPollPointValuesInNonPointsPollDoesNotGenerateMetric()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPoll.PollType = PollType.Basic;
                existingPoll.MaxPerVote = 3;
                existingPoll.MaxPoints = 7;
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Basic", MaxPerVote = 5, MaxPoints = 8 };

                Mock<IMetricEventHandler> mockMetricHandler = new Mock<IMetricEventHandler>();
                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory, mockMetricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                mockMetricHandler.Verify(m => m.PollTypeChangedEvent(It.IsAny<PollType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void IdenticalPollTypeDoesNotGenerateMetric()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPoll.PollType = PollType.Points;
                existingPoll.MaxPerVote = 3;
                existingPoll.MaxPoints = 7;
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Points", MaxPerVote = 3, MaxPoints = 7 };

                Mock<IMetricEventHandler> mockMetricHandler = new Mock<IMetricEventHandler>();
                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory, mockMetricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                mockMetricHandler.Verify(m => m.PollTypeChangedEvent(It.IsAny<PollType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void ChangingToPointsPollWithoutChangingPointsValuesGeneratesMetric()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPoll.PollType = PollType.Basic;
                existingPoll.MaxPerVote = 3;
                existingPoll.MaxPoints = 7;
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Points", MaxPerVote = 3, MaxPoints = 7 };

                Mock<IMetricEventHandler> mockMetricHandler = new Mock<IMetricEventHandler>();
                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory, mockMetricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                mockMetricHandler.Verify(m => m.PollTypeChangedEvent(PollType.Points, 3, 7, existingPoll.UUID), Times.Once());
            }
        }

        public static ManagePollTypeController CreateManagePollTypeController(IContextFactory contextFactory)
        {
            var metricHandler = new Mock<IMetricEventHandler>();
            return CreateManagePollTypeController(contextFactory, metricHandler.Object);
        }

        public static ManagePollTypeController CreateManagePollTypeController(IContextFactory contextFactory, IMetricEventHandler metricHandler)
        {
            return new ManagePollTypeController(contextFactory, metricHandler)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        private Poll CreatePoll()
        {
            return new Poll()
            {
                ManageId = PollManageGuid
            };
        }
    }
}
