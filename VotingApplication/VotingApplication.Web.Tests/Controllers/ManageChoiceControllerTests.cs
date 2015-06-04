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
    public class ManageChoiceControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("3D8E2C96-2B1B-43E2-94AF-A6E48E2B3BBD");

        [TestClass]
        public class GetTests : ManageChoiceControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                ManageChoiceController controller = CreateManageChoiceController(contextFactory);

                controller.Get(PollManageGuid);
            }

            [TestMethod]
            public void PollWithNoChoices_ReturnsEmptyList()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll poll = new Poll() { ManageId = PollManageGuid };
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                ManageChoiceController controller = CreateManageChoiceController(contextFactory);

                List<ManageChoiceResponseModel> response = controller.Get(PollManageGuid);

                CollectionAssert.AreEqual(new List<ManageChoiceResponseModel>(), response);
            }

            [TestMethod]
            public void PollWithChoices_ReturnsAll()
            {
                const int option1ChoiceNumber = 1;
                const string option1Name = "Choice 1";
                const string option1Description = "Description 1";

                const int option2ChoiceNumber = 2;
                const string option2Name = "Choice 2";
                const string option2Description = "Description 2";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();

                Poll poll = new Poll() { ManageId = PollManageGuid };
                polls.Add(poll);

                Choice option1 = new Choice()
                {
                    Name = option1Name,
                    Description = option1Description,
                    PollChoiceNumber = option1ChoiceNumber
                };
                Choice option2 = new Choice()
                {
                    Name = option2Name,
                    Description = option2Description,
                    PollChoiceNumber = option2ChoiceNumber
                };

                options.Add(option1);
                options.Add(option2);

                poll.Choices.Add(option1);
                poll.Choices.Add(option2);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                ManageChoiceController controller = CreateManageChoiceController(contextFactory);

                IEnumerable<ManageChoiceResponseModel> response = controller.Get(PollManageGuid);

                Assert.AreEqual(2, response.Count());

                ManageChoiceResponseModel responseChoice1 = response.First();
                Assert.AreEqual(option1ChoiceNumber, responseChoice1.ChoiceNumber);
                Assert.AreEqual(option1Name, responseChoice1.Name);
                Assert.AreEqual(option1Description, responseChoice1.Description);

                ManageChoiceResponseModel responseChoice2 = response.Last();
                Assert.AreEqual(option2ChoiceNumber, responseChoice2.ChoiceNumber);
                Assert.AreEqual(option2Name, responseChoice2.Name);
                Assert.AreEqual(option2Description, responseChoice2.Description);

            }
        }

        [TestClass]
        public class PutTests : ManageChoiceControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullUpdateRequest_ThrowsError()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                ManageChoiceController controller = CreateManageChoiceController(contextFactory);

                controller.Put(PollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownPollManageGuid_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                var request = new ManageChoiceUpdateRequest();

                ManageChoiceController controller = CreateManageChoiceController(contextFactory);

                controller.Put(PollManageGuid, request);
            }

            [TestMethod]
            public void ChoiceWithNoPollChoiceNumber_IsAddedToChoices()
            {
                const string optionName = "Some Choice";
                const string optionDescription = "Some Description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { ManageId = PollManageGuid });

                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageChoiceUpdateRequest();

                var optionRequest = new ChoiceUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    ChoiceNumber = null
                };
                request.Choices.Add(optionRequest);


                ManageChoiceController controller = CreateManageChoiceController(contextFactory);


                controller.Put(PollManageGuid, request);

                Assert.AreEqual(1, options.Count());
                Assert.AreEqual(optionName, options.First().Name);
                Assert.AreEqual(optionDescription, options.First().Description);
            }

            [TestMethod]
            public void ChoiceWithNoPollChoiceNumber_IsAddedToPoll()
            {
                const string optionName = "Some Choice";
                const string optionDescription = "Some Description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();

                var poll = new Poll() { ManageId = PollManageGuid };
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageChoiceUpdateRequest();

                var optionRequest = new ChoiceUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    ChoiceNumber = null
                };
                request.Choices.Add(optionRequest);


                ManageChoiceController controller = CreateManageChoiceController(contextFactory);


                controller.Put(PollManageGuid, request);

                Assert.AreEqual(1, poll.Choices.Count());
            }

            [TestMethod]
            public void ChoiceWithPollChoiceNumber_IsUpdated()
            {
                const string optionName = "Some Choice";
                const string optionDescription = "Some Description";
                const int pollChoiceNumber = 2;

                const string newChoiceName = "Some other option";
                const string newChoiceDescription = "Some other description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };


                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                var option = new Choice()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollChoiceNumber = pollChoiceNumber
                };

                poll.Choices.Add(option);

                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageChoiceUpdateRequest();

                var optionRequest = new ChoiceUpdate()
                {
                    Name = newChoiceName,
                    Description = newChoiceDescription,
                    ChoiceNumber = pollChoiceNumber
                };
                request.Choices.Add(optionRequest);


                ManageChoiceController controller = CreateManageChoiceController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(newChoiceName, options.First().Name);
                Assert.AreEqual(newChoiceDescription, options.First().Description);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void ChoiceWithUnknownPollChoiceNumber_ThrowsNotFound()
            {
                const string optionName = "Some Choice";
                const string optionDescription = "Some Description";
                const int pollChoiceNumber = 2;
                const int unknownPollChoiceNumber = 3;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                var option = new Choice()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollChoiceNumber = pollChoiceNumber
                };

                poll.Choices.Add(option);

                options.Add(option);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageChoiceUpdateRequest();
                var optionRequest = new ChoiceUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    ChoiceNumber = unknownPollChoiceNumber
                };
                request.Choices.Add(optionRequest);


                ManageChoiceController controller = CreateManageChoiceController(contextFactory);


                controller.Put(PollManageGuid, request);
            }

            [TestMethod]
            public void ChoiceWithPollChoiceNumber_DoesNotAddNewChoice()
            {
                const string optionName = "Some Choice";
                const string optionDescription = "Some Description";
                const int pollChoiceNumber = 2;

                const string newChoiceName = "Some other option";
                const string newChoiceDescription = "Some other description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };


                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                var option = new Choice()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollChoiceNumber = pollChoiceNumber
                };

                poll.Choices.Add(option);

                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageChoiceUpdateRequest();

                var optionRequest = new ChoiceUpdate()
                {
                    Name = newChoiceName,
                    Description = newChoiceDescription,
                    ChoiceNumber = pollChoiceNumber
                };
                request.Choices.Add(optionRequest);


                ManageChoiceController controller = CreateManageChoiceController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(1, options.Count());
            }

            [TestMethod]
            public void ChoiceNotInRequest_RemovesChoiceAndVotes()
            {
                const string optionName = "Some Choice";
                const string optionDescription = "Some Description";
                const int pollChoiceNumber = 2;


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };


                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                var option = new Choice()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollChoiceNumber = pollChoiceNumber
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot();
                var ballot2 = new Ballot();

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Id = 1 };
                var vote2 = new Vote() { Id = 2 };

                vote1.Choice = option;
                vote2.Choice = option;
                ballot1.Votes.Add(vote1);
                ballot2.Votes.Add(vote2);
                poll.Choices.Add(option);

                votes.Add(vote1);
                votes.Add(vote2);
                ballots.Add(ballot1);
                ballots.Add(ballot2);
                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                var request = new ManageChoiceUpdateRequest();

                ManageChoiceController controller = CreateManageChoiceController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(0, poll.Choices.Count());
                Assert.AreEqual(0, options.Count());
                Assert.AreEqual(0, votes.Count());
                Assert.AreEqual(0, ballot1.Votes.Count());
                Assert.AreEqual(0, ballot2.Votes.Count());
            }

            [TestMethod]
            public void MultipleRequests()
            {
                const string optionRemoveName = "Some Choice to remove";
                const string optionRemoveDescription = "Some Description to remove";
                const int optionRemoveNumber = 2;

                const string optionUpdateName = "Some Choice to update";
                const string optionUpdateDescription = "Some Description to update";
                const int optionUpdateNumber = 1;

                const string updatedName = "An updated Name";
                const string updatedDescription = "An updated Description";

                const string addName = "A new option";
                const string addDescription = "A new description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();

                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };
                polls.Add(poll);

                var optionToRemove = new Choice()
                {
                    Name = optionRemoveName,
                    Description = optionRemoveDescription,
                    PollChoiceNumber = optionRemoveNumber
                };
                var optionToUpdate = new Choice()
                {
                    Name = optionUpdateName,
                    Description = optionUpdateDescription,
                    PollChoiceNumber = optionUpdateNumber
                };
                options.Add(optionToRemove);
                options.Add(optionToUpdate);

                var ballot = new Ballot();
                ballots.Add(ballot);

                var vote = new Vote();
                votes.Add(vote);

                vote.Choice = optionToRemove;
                ballot.Votes.Add(vote);
                poll.Choices.Add(optionToRemove);
                poll.Choices.Add(optionToUpdate);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                var request = new ManageChoiceUpdateRequest();
                var updateChoice = new ChoiceUpdate()
                {
                    Name = updatedName,
                    Description = updatedDescription,
                    ChoiceNumber = optionUpdateNumber
                };
                var addChoice = new ChoiceUpdate()
                {
                    Name = addName,
                    Description = addDescription,
                    ChoiceNumber = null
                };
                request.Choices.Add(updateChoice);
                request.Choices.Add(addChoice);

                ManageChoiceController controller = CreateManageChoiceController(contextFactory);


                controller.Put(PollManageGuid, request);


                Assert.AreEqual(2, poll.Choices.Count());
                Assert.AreEqual(2, options.Count());

                Assert.AreEqual(addName, options.First().Name);
                Assert.AreEqual(addDescription, options.First().Description);

                Assert.AreEqual(updatedName, options.Last().Name);
                Assert.AreEqual(updatedDescription, options.Last().Description);
                Assert.AreEqual(optionUpdateNumber, options.Last().PollChoiceNumber);
            }

            [TestMethod]
            public void AddingAnChoiceGeneratesAnAddChoiceMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageChoiceController controller = CreateManageChoiceController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Choices = new List<Choice>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                ChoiceUpdate optionUpdate = new ChoiceUpdate() { Name = "New Choice" };
                ManageChoiceUpdateRequest request = new ManageChoiceUpdateRequest() { Choices = new List<ChoiceUpdate>() { optionUpdate } };

                // Act
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleChoiceAddedEvent(It.Is<Choice>(o => o.Name == "New Choice"), existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleChoiceDeletedEvent(It.IsAny<Choice>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleChoiceUpdatedEvent(It.IsAny<Choice>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void UpdatingAnChoiceGeneratesAnUpdateChoiceMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageChoiceController controller = CreateManageChoiceController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Choices = new List<Choice>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                Choice existingChoice = new Choice() { Name = "New Choice", PollChoiceNumber = 1 };
                existingPoll.Choices.Add(existingChoice);
                ChoiceUpdate choiceUpdate = new ChoiceUpdate() { Name = "New Choice", ChoiceNumber = 1 };
                ManageChoiceUpdateRequest request = new ManageChoiceUpdateRequest() { Choices = new List<ChoiceUpdate>() { choiceUpdate } };

                // Act
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleChoiceUpdatedEvent(existingChoice, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleChoiceAddedEvent(It.IsAny<Choice>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleChoiceDeletedEvent(It.IsAny<Choice>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void DeletingAnChoiceGeneratesADeleteChoiceMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageChoiceController controller = CreateManageChoiceController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll() { Choices = new List<Choice>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                Choice existingChoice = new Choice() { Name = "New Choice", PollChoiceNumber = 1 };
                existingPoll.Choices.Add(existingChoice);
                ManageChoiceUpdateRequest request = new ManageChoiceUpdateRequest() { Choices = new List<ChoiceUpdate>() };

                // Act
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleChoiceDeletedEvent(existingChoice, existingPoll.UUID), Times.Once());
                metricHandler.Verify(m => m.HandleChoiceAddedEvent(It.IsAny<Choice>(), It.IsAny<Guid>()), Times.Never());
                metricHandler.Verify(m => m.HandleChoiceUpdatedEvent(It.IsAny<Choice>(), It.IsAny<Guid>()), Times.Never());
            }

            [TestMethod]
            public void DeletingAnChoiceWithVotesGeneratesADeleteVoteMetric()
            {
                // Arrange
                var metricHandler = new Mock<IMetricHandler>();
                var polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageChoiceController controller = CreateManageChoiceController(contextFactory, metricHandler.Object);

                Poll existingPoll = new Poll { Choices = new List<Choice>(), UUID = Guid.NewGuid(), ManageId = Guid.NewGuid() };
                polls.Add(existingPoll);

                Choice existingChoice = new Choice { PollChoiceNumber = 1 };
                existingPoll.Choices.Add(existingChoice);

                Vote existingVote = new Vote() { Poll = existingPoll, Choice = existingChoice, VoteValue = 1 };
                votes.Add(existingVote);

                Ballot existingBallot = new Ballot() { Votes = new List<Vote>() { existingVote } };
                ballots.Add(existingBallot);

                // Act
                ManageChoiceUpdateRequest request = new ManageChoiceUpdateRequest() { Choices = new List<ChoiceUpdate>() };
                controller.Put(existingPoll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.HandleVoteDeletedEvent(existingVote, existingPoll.UUID), Times.Once());
            }
        }

        public static ManageChoiceController CreateManageChoiceController(IContextFactory contextFactory)
        {
            var metricHandler = new Mock<IMetricHandler>();
            return CreateManageChoiceController(contextFactory, metricHandler.Object);
        }

        public static ManageChoiceController CreateManageChoiceController(IContextFactory contextFactory, IMetricHandler metricHandler)
        {
            return new ManageChoiceController(contextFactory, metricHandler)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }
    }
}
