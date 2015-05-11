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
using VotingApplication.Web.Api.Tests.TestHelpers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class PollOptionControllerTests
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
                PollOptionController controller = CreatePollOptionController(contextFactory);


                controller.Post(PollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollOptionController controller = CreatePollOptionController(contextFactory);

                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel();

                controller.Post(PollManageGuid, optionCreationRequestModel);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.MethodNotAllowed)]
            public void OptionAddingNotAllowedForPoll_ThrowsMethodNotAllowed()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { UUID = PollManageGuid, OptionAdding = false });

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollOptionController controller = CreatePollOptionController(contextFactory);

                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel();

                controller.Post(PollManageGuid, optionCreationRequestModel);
            }

            [TestMethod]
            public void OptionAddingAllowedForPoll_AddsOption()
            {
                const string optionName = "new option";
                const string optionDescription = "description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();

                polls.Add(new Poll() { UUID = PollManageGuid, OptionAdding = true });


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);
                PollOptionController controller = CreatePollOptionController(contextFactory);


                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel()
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
            public void OptionAddingAllowedForPoll_AddsOptionToPoll()
            {
                const string optionName = "new option";
                const string optionDescription = "description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();

                Poll poll = new Poll()
                {
                    UUID = PollManageGuid,
                    OptionAdding = true
                };
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);
                PollOptionController controller = CreatePollOptionController(contextFactory);


                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel()
                {
                    Name = optionName,
                    Description = optionDescription,
                };

                controller.Post(PollManageGuid, optionCreationRequestModel);

                Assert.AreEqual(1, poll.Options.Count);
                Assert.AreEqual(optionName, poll.Options.First().Name);
                Assert.AreEqual(optionDescription, poll.Options.First().Description);
            }

            [TestMethod]
            public void AddingOptionGeneratesMetric()
            {
                // Arrange
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                var metricHandler = new Mock<IMetricHandler>();
                PollOptionController controller = CreatePollOptionController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Options = new List<Option>(), UUID = Guid.NewGuid(), OptionAdding = true };
                polls.Add(existingPoll);

                OptionCreationRequestModel request = new OptionCreationRequestModel() { Name = "New Option" };

                // Act
                controller.Post(existingPoll.UUID, request);

                // Assert
                metricHandler.Verify(m => m.HandleOptionAddedEvent(It.Is<Option>(o => o.Name == "New Option"), existingPoll.UUID), Times.Once());
            }

            public static PollOptionController CreatePollOptionController(IContextFactory contextFactory)
            {
                var metricHandler = new Mock<IMetricHandler>();
                return CreatePollOptionController(contextFactory, metricHandler.Object);
            }

            public static PollOptionController CreatePollOptionController(IContextFactory contextFactory, IMetricHandler metricHandler)
            {
                return new PollOptionController(contextFactory, metricHandler)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }
        }
    }
}
