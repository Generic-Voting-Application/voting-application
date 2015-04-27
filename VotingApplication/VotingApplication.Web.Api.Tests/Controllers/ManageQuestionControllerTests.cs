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
    public class ManageQuestionControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("961efb70-6767-4658-a95d-fea312c802ec");

        [TestClass]
        public class PutTests : ManageQuestionControllerTests
        {
            private ManageQuestionController _controller;
            private Mock<IMetricHandler> _metricHandler;
            private Poll _existingPoll;

            [TestInitialize]
            public void Setup()
            {
                _existingPoll = new Poll() { ManageId = PollManageGuid, Name = "ABC" };

                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                existingPolls.Add(_existingPoll);

                _metricHandler = new Mock<IMetricHandler>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                _controller = CreateManageQuestionController(contextFactory, _metricHandler.Object);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageIdReturnsNotFound()
            {
                // Arrange
                ManageQuestionRequest request = new ManageQuestionRequest { };
                
                // Act
                _controller.Put(Guid.NewGuid(), request);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullRequestReturnsBadRequest()
            {
                // Act
                _controller.Put(PollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullQuestionReturnsBadRequest()
            {
                // Arrange
                ManageQuestionRequest request = new ManageQuestionRequest { Question = null };

                // Act
                _controller.Put(PollManageGuid, request);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void EmptyQuestionReturnsBadRequest()
            {
                // Arrange
                ManageQuestionRequest request = new ManageQuestionRequest { Question = "" };

                // Act
                _controller.Put(PollManageGuid, request);
            }

            [TestMethod]
            public void ValidQuestionSetsPollName()
            {
                // Arrange
                ManageQuestionRequest request = new ManageQuestionRequest { Question = "DEF" };

                // Act
                _controller.Put(PollManageGuid, request);

                // Assert
                Assert.AreEqual(_existingPoll.Name, "DEF");
            }

            [TestMethod]
            public void ValidQuestionGeneratesMetric()
            {
                // Arrange
                ManageQuestionRequest request = new ManageQuestionRequest { Question = "JKL" };

                // Act
                _controller.Put(PollManageGuid, request);

                // Assert
                _metricHandler.Verify(m => m.QuestionChangedEvent("JKL", _existingPoll.UUID), Times.Once());
            }

            [TestMethod]
            public void UnchangedQuestionDoesNotGenerateMetric()
            {
                // Arrange
                ManageQuestionRequest request = new ManageQuestionRequest { Question = "ABC" };

                // Act
                _controller.Put(PollManageGuid, request);

                // Assert
                _metricHandler.Verify(m => m.QuestionChangedEvent(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void InvalidQuestionDoesNotGenerateMetric()
            {
                // Arrange
                ManageQuestionRequest request = new ManageQuestionRequest { Question = "" };

                // Act
                _controller.Put(PollManageGuid, request);

                // Assert
                _metricHandler.Verify(m => m.QuestionChangedEvent(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
            }
        }

        public static ManageQuestionController CreateManageQuestionController(IContextFactory contextFactory, IMetricHandler metricHandler)
        {
            return new ManageQuestionController(contextFactory, metricHandler)
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
