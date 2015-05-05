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
    public class ManageOptionControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("3D8E2C96-2B1B-43E2-94AF-A6E48E2B3BBD");

        [TestClass]
        public class GetTests : ManageOptionControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                ManageOptionController controller = CreateManageOptionController(contextFactory);

                controller.Get(PollManageGuid);
            }

            [TestMethod]
            public void PollWithNoOptions_ReturnsEmptyList()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll poll = new Poll() { ManageId = PollManageGuid };
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                ManageOptionController controller = CreateManageOptionController(contextFactory);

                List<ManageOptionResponseModel> response = controller.Get(PollManageGuid);

                CollectionAssert.AreEqual(new List<ManageOptionResponseModel>(), response);
            }

            [TestMethod]
            public void PollWithOptions_ReturnsAll()
            {
                const int option1OptionNumber = 1;
                const string option1Name = "Option 1";
                const string option1Description = "Description 1";

                const int option2OptionNumber = 2;
                const string option2Name = "Option 2";
                const string option2Description = "Description 2";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();

                Poll poll = new Poll() { ManageId = PollManageGuid };
                polls.Add(poll);

                Option option1 = new Option()
                {
                    Name = option1Name,
                    Description = option1Description,
                    PollOptionNumber = option1OptionNumber
                };
                Option option2 = new Option()
                {
                    Name = option2Name,
                    Description = option2Description,
                    PollOptionNumber = option2OptionNumber
                };

                options.Add(option1);
                options.Add(option2);

                poll.Options.Add(option1);
                poll.Options.Add(option2);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                ManageOptionController controller = CreateManageOptionController(contextFactory);

                IEnumerable<ManageOptionResponseModel> response = controller.Get(PollManageGuid);

                Assert.AreEqual(2, response.Count());

                ManageOptionResponseModel responseOption1 = response.First();
                Assert.AreEqual(option1OptionNumber, responseOption1.OptionNumber);
                Assert.AreEqual(option1Name, responseOption1.Name);
                Assert.AreEqual(option1Description, responseOption1.Description);

                ManageOptionResponseModel responseOption2 = response.Last();
                Assert.AreEqual(option2OptionNumber, responseOption2.OptionNumber);
                Assert.AreEqual(option2Name, responseOption2.Name);
                Assert.AreEqual(option2Description, responseOption2.Description);

            }
        }

        [TestClass]
        public class PutTests : ManageOptionControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullUpdateRequest_ThrowsError()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                ManageOptionController controller = CreateManageOptionController(contextFactory);

                controller.Put(PollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownPollManageGuid_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                var request = new ManageOptionUpdateRequest();

                ManageOptionController controller = CreateManageOptionController(contextFactory);

                controller.Put(PollManageGuid, request);
            }

            [TestMethod]
            public void OptionWithNoPollOptionNumber_IsAddedToOptions()
            {
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { ManageId = PollManageGuid });

                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    OptionNumber = null
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(PollManageGuid, request);

                Assert.AreEqual(1, options.Count());
                Assert.AreEqual(optionName, options.First().Name);
                Assert.AreEqual(optionDescription, options.First().Description);
            }

            [TestMethod]
            public void OptionWithNoPollOptionNumber_IsAddedToPoll()
            {
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();

                var poll = new Poll() { ManageId = PollManageGuid };
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    OptionNumber = null
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(PollManageGuid, request);

                Assert.AreEqual(1, poll.Options.Count());
            }

            [TestMethod]
            public void OptionWithPollOptionNumber_IsUpdated()
            {
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;

                const string newOptionName = "Some other option";
                const string newOptionDescription = "Some other description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };


                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                poll.Options.Add(option);

                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = newOptionName,
                    Description = newOptionDescription,
                    OptionNumber = pollOptionNumber
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(newOptionName, options.First().Name);
                Assert.AreEqual(newOptionDescription, options.First().Description);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void OptionWithUnknownPollOptionNumber_ThrowsNotFound()
            {
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;
                const int unknownPollOptionNumber = 3;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                poll.Options.Add(option);

                options.Add(option);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();
                var optionRequest = new OptionUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    OptionNumber = unknownPollOptionNumber
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(PollManageGuid, request);
            }

            [TestMethod]
            public void OptionWithPollOptionNumber_DoesNotAddNewOption()
            {
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;

                const string newOptionName = "Some other option";
                const string newOptionDescription = "Some other description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };


                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                poll.Options.Add(option);

                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = newOptionName,
                    Description = newOptionDescription,
                    OptionNumber = pollOptionNumber
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(1, options.Count());
            }

            [TestMethod]
            public void OptionNotInRequest_RemovesOptionAndVotes()
            {
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };


                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot();
                var ballot2 = new Ballot();

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Id = 1 };
                var vote2 = new Vote() { Id = 2 };

                vote1.Option = option;
                vote2.Option = option;
                ballot1.Votes.Add(vote1);
                ballot2.Votes.Add(vote2);
                poll.Options.Add(option);

                votes.Add(vote1);
                votes.Add(vote2);
                ballots.Add(ballot1);
                ballots.Add(ballot2);
                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                var request = new ManageOptionUpdateRequest();

                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(0, poll.Options.Count());
                Assert.AreEqual(0, options.Count());
                Assert.AreEqual(0, votes.Count());
                Assert.AreEqual(0, ballot1.Votes.Count());
                Assert.AreEqual(0, ballot2.Votes.Count());
            }

            [TestMethod]
            public void MultipleRequests()
            {
                const string optionRemoveName = "Some Option to remove";
                const string optionRemoveDescription = "Some Description to remove";
                const int optionRemoveNumber = 2;

                const string optionUpdateName = "Some Option to update";
                const string optionUpdateDescription = "Some Description to update";
                const int optionUpdateNumber = 1;

                const string updatedName = "An updated Name";
                const string updatedDescription = "An updated Description";

                const string addName = "A new option";
                const string addDescription = "A new description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();

                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };
                polls.Add(poll);

                var optionToRemove = new Option()
                {
                    Name = optionRemoveName,
                    Description = optionRemoveDescription,
                    PollOptionNumber = optionRemoveNumber
                };
                var optionToUpdate = new Option()
                {
                    Name = optionUpdateName,
                    Description = optionUpdateDescription,
                    PollOptionNumber = optionUpdateNumber
                };
                options.Add(optionToRemove);
                options.Add(optionToUpdate);

                var ballot = new Ballot();
                ballots.Add(ballot);

                var vote = new Vote();
                votes.Add(vote);

                vote.Option = optionToRemove;
                ballot.Votes.Add(vote);
                poll.Options.Add(optionToRemove);
                poll.Options.Add(optionToUpdate);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                var request = new ManageOptionUpdateRequest();
                var updateOption = new OptionUpdate()
                {
                    Name = updatedName,
                    Description = updatedDescription,
                    OptionNumber = optionUpdateNumber
                };
                var addOption = new OptionUpdate()
                {
                    Name = addName,
                    Description = addDescription,
                    OptionNumber = null
                };
                request.Options.Add(updateOption);
                request.Options.Add(addOption);

                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(2, poll.Options.Count());
                Assert.AreEqual(2, options.Count());

                Assert.AreEqual(addName, options.First().Name);
                Assert.AreEqual(addDescription, options.First().Description);

                Assert.AreEqual(updatedName, options.Last().Name);
                Assert.AreEqual(updatedDescription, options.Last().Description);
                Assert.AreEqual(optionUpdateNumber, options.Last().PollOptionNumber);
            }

            [TestMethod]
            public void AddingAnOptionGeneratesAnAddOptionMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageOptionController controller = CreateManageOptionController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Options = new List<Option>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                OptionUpdate optionUpdate = new OptionUpdate() { Name = "New Option" };
                ManageOptionUpdateRequest request = new ManageOptionUpdateRequest() { Options = new List<OptionUpdate>() { optionUpdate }};

                // Act
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleOptionAddedEvent(It.Is<Option>(o => o.Name == "New Option"), existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleOptionDeletedEvent(It.IsAny<Option>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleOptionUpdatedEvent(It.IsAny<Option>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void UpdatingAnOptionGeneratesAnUpdateOptionMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageOptionController controller = CreateManageOptionController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Options = new List<Option>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                Option existingOption = new Option() { Name = "New Option", PollOptionNumber = 1 };
                existingPoll.Options.Add(existingOption);
                OptionUpdate optionUpdate = new OptionUpdate() { Name = "New Option", OptionNumber = 1 };
                ManageOptionUpdateRequest request = new ManageOptionUpdateRequest() { Options = new List<OptionUpdate>() { optionUpdate } };

                // Act
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleOptionUpdatedEvent(existingOption, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleOptionAddedEvent(It.IsAny<Option>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleOptionDeletedEvent(It.IsAny<Option>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void DeletingAnOptionGeneratesADeleteOptionMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageOptionController controller = CreateManageOptionController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Options = new List<Option>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                Option existingOption = new Option() { Name = "New Option", PollOptionNumber = 1 };
                existingPoll.Options.Add(existingOption);
                ManageOptionUpdateRequest request = new ManageOptionUpdateRequest() { Options = new List<OptionUpdate>() };

                // Act
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleOptionDeletedEvent(existingOption, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleOptionAddedEvent(It.IsAny<Option>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleOptionUpdatedEvent(It.IsAny<Option>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void DeletingAnOptionWithVotesGeneratesADeleteVoteMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                var polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageOptionController controller = CreateManageOptionController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll { Options = new List<Option>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                Option existingOption = new Option { PollOptionNumber = 1 };
                existingPoll.Options.Add(existingOption);

                Vote existingVote = new Vote() { Poll = existingPoll, Option = existingOption, VoteValue = 1 };
                votes.Add(existingVote);

                Ballot existingBallot = new Ballot() { Votes = new List<Vote>() { existingVote } };
                ballots.Add(existingBallot);

                // Act
                ManageOptionUpdateRequest request = new ManageOptionUpdateRequest() { Options = new List<OptionUpdate>() };
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleVoteDeletedEvent(existingVote, existingPoll.UUID), Times.Once());
            }
        }

        public static ManageOptionController CreateManageOptionController(IContextFactory contextFactory)
        {
            var metricHandler = new Mock<IMetricHandler>();
            return CreateManageOptionController(contextFactory, metricHandler.Object);
        }

        public static ManageOptionController CreateManageOptionController(IContextFactory contextFactory, IMetricHandler metricHandler)
        {
            return new ManageOptionController(contextFactory, metricHandler)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }
    }
}
