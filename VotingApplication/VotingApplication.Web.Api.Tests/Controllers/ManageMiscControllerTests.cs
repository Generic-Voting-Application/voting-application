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
using VotingApplication.Web.Tests.TestHelpers;

namespace VotingApplication.Web.Tests.Controllers
{
    [TestClass]
    public class ManageMiscControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("961efb70-6767-4658-a95d-fea312c802ec");

        [TestClass]
        public class PutTests : ManageMiscControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { };

                ManageMiscController controller = CreateManageExpiryController(contextFactory, new Mock<IMetricHandler>().Object);

                controller.Put(Guid.NewGuid(), request);
            }

            [TestMethod]
            public void SetMiscConfig_SetsMiscSettings()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                existingPoll.NamedVoting = false;
                existingPoll.InviteOnly = false;
                existingPoll.ChoiceAdding = false;
                existingPoll.IsElectionMode = false;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { InviteOnly = true, NamedVoting = true, ChoiceAdding = true, IsElectionMode = true };

                ManageMiscController controller = CreateManageExpiryController(contextFactory, new Mock<IMetricHandler>().Object);

                controller.Put(PollManageGuid, request);

                Assert.IsTrue(existingPoll.InviteOnly);
                Assert.IsTrue(existingPoll.NamedVoting);
                Assert.IsTrue(existingPoll.ChoiceAdding);
                Assert.IsTrue(existingPoll.IsElectionMode);
            }

            [TestMethod]
            public void AlteringMiscConfigGeneratesMetrics()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                existingPoll.NamedVoting = false;
                existingPoll.InviteOnly = false;
                existingPoll.ChoiceAdding = false;
                existingPoll.IsElectionMode = false;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { InviteOnly = true, NamedVoting = true, ChoiceAdding = true, IsElectionMode = true };

                Mock<IMetricHandler> metricHandler = new Mock<IMetricHandler>();
                ManageMiscController controller = CreateManageExpiryController(contextFactory, metricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                metricHandler.Verify(m => m.HandleInviteOnlyChangedEvent(true, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleNamedVotingChangedEvent(true, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleChoiceAddingChangedEvent(true, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleIsElectionModeChangedEvent(true, existingPoll.UUID), Times.Once());
            }

            [TestMethod]
            public void SettingExistingMiscSettingsDoesNotGenerateMetrics()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                existingPoll.NamedVoting = false;
                existingPoll.InviteOnly = false;
                existingPoll.ChoiceAdding = false;
                existingPoll.IsElectionMode = false;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { InviteOnly = false, NamedVoting = false, ChoiceAdding = false, IsElectionMode = false };

                Mock<IMetricHandler> metricHandler = new Mock<IMetricHandler>();
                ManageMiscController controller = CreateManageExpiryController(contextFactory, metricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                metricHandler.Verify(m => m.HandleInviteOnlyChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleNamedVotingChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleChoiceAddingChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleIsElectionModeChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
            }
        }

        public static ManageMiscController CreateManageExpiryController(IContextFactory contextFactory, IMetricHandler metricHandler)
        {
            return new ManageMiscController(contextFactory, metricHandler)
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
