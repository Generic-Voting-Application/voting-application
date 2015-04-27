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
                existingPoll.OptionAdding = false;
                existingPoll.HiddenResults = false;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { InviteOnly = true, NamedVoting = true, OptionAdding = true, HiddenResults = true };

                ManageMiscController controller = CreateManageExpiryController(contextFactory, new Mock<IMetricHandler>().Object);

                controller.Put(PollManageGuid, request);

                Assert.IsTrue(existingPoll.InviteOnly);
                Assert.IsTrue(existingPoll.NamedVoting);
                Assert.IsTrue(existingPoll.OptionAdding);
                Assert.IsTrue(existingPoll.HiddenResults);
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
                existingPoll.OptionAdding = false;
                existingPoll.HiddenResults = false;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { InviteOnly = true, NamedVoting = true, OptionAdding = true, HiddenResults = true };

                Mock<IMetricHandler> metricHandler = new Mock<IMetricHandler>();
                ManageMiscController controller = CreateManageExpiryController(contextFactory, metricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                metricHandler.Verify(m => m.InviteOnlyChangedEvent(true, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.NamedVotingChangedEvent(true, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.OptionAddingChangedEvent(true, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HiddenResultsChangedEvent(true, existingPoll.UUID), Times.Once());
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
                existingPoll.OptionAdding = false;
                existingPoll.HiddenResults = false;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { InviteOnly = false, NamedVoting = false, OptionAdding = false, HiddenResults = false };

                Mock<IMetricHandler> metricHandler = new Mock<IMetricHandler>();
                ManageMiscController controller = CreateManageExpiryController(contextFactory, metricHandler.Object);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                metricHandler.Verify(m => m.InviteOnlyChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.NamedVotingChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.OptionAddingChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HiddenResultsChangedEvent(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never());
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
