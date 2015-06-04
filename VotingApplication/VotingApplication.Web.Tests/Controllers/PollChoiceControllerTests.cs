using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    public class PollChoiceControllerTests
    {
        [TestClass]
        public class PostTests
        {
            public readonly Guid PollManageGuid = new Guid("EBDE4ED9-D014-4145-B998-13B5A247BB4B");

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullRequest_ThrowsBadRequest()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollChoiceController controller = CreatePollChoiceController(contextFactory);


                controller.Post(PollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollChoiceController controller = CreatePollChoiceController(contextFactory);

                ChoiceCreationRequestModel optionCreationRequestModel = new ChoiceCreationRequestModel();

                controller.Post(PollManageGuid, optionCreationRequestModel);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.MethodNotAllowed)]
            public void ChoiceAddingNotAllowedForPoll_ThrowsMethodNotAllowed()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { UUID = PollManageGuid, ChoiceAdding = false });

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollChoiceController controller = CreatePollChoiceController(contextFactory);

                ChoiceCreationRequestModel optionCreationRequestModel = new ChoiceCreationRequestModel();

                controller.Post(PollManageGuid, optionCreationRequestModel);
            }

            [TestMethod]
            public void ChoiceAddingAllowedForPoll_AddsChoice()
            {
                const string optionName = "new option";
                const string optionDescription = "description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();

                polls.Add(new Poll() { UUID = PollManageGuid, ChoiceAdding = true });


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);
                PollChoiceController controller = CreatePollChoiceController(contextFactory);


                ChoiceCreationRequestModel optionCreationRequestModel = new ChoiceCreationRequestModel()
                {
                    Name = optionName,
                    Description = optionDescription,
                };

                controller.Post(PollManageGuid, optionCreationRequestModel);

                Assert.AreEqual(1, options.Count());
                Assert.AreEqual(optionName, options.First().Name);
                Assert.AreEqual(optionDescription, options.First().Description);
            }

            [TestMethod]
            public void ChoiceAddingAllowedForPoll_AddsChoiceToPoll()
            {
                const string optionName = "new option";
                const string optionDescription = "description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();

                Poll poll = new Poll()
                {
                    UUID = PollManageGuid,
                    ChoiceAdding = true
                };
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);
                PollChoiceController controller = CreatePollChoiceController(contextFactory);


                ChoiceCreationRequestModel optionCreationRequestModel = new ChoiceCreationRequestModel()
                {
                    Name = optionName,
                    Description = optionDescription,
                };

                controller.Post(PollManageGuid, optionCreationRequestModel);

                Assert.AreEqual(1, poll.Choices.Count);
                Assert.AreEqual(optionName, poll.Choices.First().Name);
                Assert.AreEqual(optionDescription, poll.Choices.First().Description);
            }

            [TestMethod]
            public void AddingChoiceGeneratesMetric()
            {
                // Arrange
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                var metricHandler = new Mock<IMetricHandler>();
                PollChoiceController controller = CreatePollChoiceController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Choices = new List<Choice>(), UUID = Guid.NewGuid(), ChoiceAdding = true };
                polls.Add(existingPoll);

                ChoiceCreationRequestModel request = new ChoiceCreationRequestModel() { Name = "New Choice" };

                // Act
                controller.Post(existingPoll.UUID, request);

                // Assert
                metricHandler.Verify(m => m.HandleChoiceAddedEvent(It.Is<Choice>(o => o.Name == "New Choice"), existingPoll.UUID), Times.Once());
            }

            public static PollChoiceController CreatePollChoiceController(IContextFactory contextFactory)
            {
                var metricHandler = new Mock<IMetricHandler>();
                return CreatePollChoiceController(contextFactory, metricHandler.Object);
            }

            public static PollChoiceController CreatePollChoiceController(IContextFactory contextFactory, IMetricHandler metricHandler)
            {
                return new PollChoiceController(contextFactory, metricHandler)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }
        }
    }
}
