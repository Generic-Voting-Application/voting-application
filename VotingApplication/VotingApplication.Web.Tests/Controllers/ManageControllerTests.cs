using FakeDbSet;
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
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Tests.TestHelpers;

namespace VotingApplication.Web.Tests.Controllers
{
    [TestClass]
    public class ManageControllerTests
    {
        private ManageController _controller;
        private Guid _manageMainUUID;
        private Guid _manageEmptyUUID;
        private Choice _burgerChoice;
        private Choice _pizzaChoice;
        private Vote _burgerVote;
        private Poll _mainPoll;
        private InMemoryDbSet<Choice> _dummyChoices;
        private InMemoryDbSet<Vote> _dummyVotes;
        private InMemoryDbSet<Poll> _dummyPolls;
        private InMemoryDbSet<Ballot> _dummyTokens;

        [TestInitialize]
        public void setup()
        {
            Guid mainUUID = Guid.NewGuid();
            Guid emptyUUID = Guid.NewGuid();
            _manageMainUUID = Guid.NewGuid();
            _manageEmptyUUID = Guid.NewGuid();

            _burgerChoice = new Choice { Id = 1, Name = "Burger King" };
            _pizzaChoice = new Choice { Id = 2, Name = "Pizza Hut" };
            _dummyChoices = new InMemoryDbSet<Choice>(true);
            _dummyChoices.Add(_burgerChoice);
            _dummyChoices.Add(_pizzaChoice);

            _burgerVote = new Vote() { Id = 1, Poll = new Poll() { UUID = mainUUID }, Choice = new Choice() { Id = 1 } };
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            _dummyVotes.Add(_burgerVote);

            _dummyPolls = new InMemoryDbSet<Poll>(true);
            _mainPoll = new Poll() { UUID = mainUUID, ManageId = _manageMainUUID, Choices = new List<Choice>() { _burgerChoice, _pizzaChoice }, Ballots = new List<Ballot>() };
            Poll emptyPoll = new Poll() { UUID = emptyUUID, ManageId = _manageEmptyUUID, Choices = new List<Choice>(), Ballots = new List<Ballot>() };
            _dummyPolls.Add(_mainPoll);
            _dummyPolls.Add(emptyPoll);

            _dummyTokens = new InMemoryDbSet<Ballot>(true);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Choices).Returns(_dummyChoices);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Ballots).Returns(_dummyTokens);

            _controller = new ManageController(mockContextFactory.Object, null);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyChoices.Count(); i++)
            {
                _dummyChoices.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            _controller.Get(_manageMainUUID);
        }


        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetWithNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);
        }

        [TestMethod]
        public void GetWithEmptyPollReturnsEmptyChoiceList()
        {
            // Act
            var response = _controller.Get(_manageEmptyUUID);

            // Assert
            Assert.AreEqual(0, response.Choices.Count);
        }

        [TestMethod]
        public void GetWithPopulatedPollReturnsChoicesForThatPoll()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            Assert.AreEqual(2, response.Choices.Count);
            CollectionAssert.AreEqual(new string[] { "Burger King", "Pizza Hut" }, response.Choices.Select(r => r.Name).ToArray());
        }

        [TestMethod]
        public void GetWithInviteesReturnsCountOfInvitees()
        {
            // Arrange
            Ballot emailBallot = new Ballot() { Email = "a@b.c" };
            Ballot nullBallot = new Ballot() { Email = null };
            Ballot emptyBallot = new Ballot() { Email = "" };

            _mainPoll.Ballots = new List<Ballot> { emailBallot, nullBallot, emptyBallot };

            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            Assert.AreEqual(1, response.InviteeCount);
        }

        #endregion

        [TestClass]
        public class GetTests
        {
            [TestMethod]
            public void BallotWithVotes_IsReturned()
            {
                var pollManageGuid = new Guid("992E252C-56DB-4E9D-9A31-F5BBA3FA5361");

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = pollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = new Guid("96ADD6EF-DFE4-4160-84C5-63EDC3076A1B")
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote()
                {
                    Choice = new Choice()
                    {
                        PollChoiceNumber = 1,

                    },
                    VoteValue = 1
                };

                poll.Ballots.Add(ballot);
                ballot.Votes.Add(vote);

                votes.Add(vote);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageController controller = CreateManageController(contextFactory);


                ManagePollRequestResponseModel response = controller.Get(pollManageGuid);


                Assert.AreEqual(1, response.VotersCount);
            }

            [TestMethod]
            public void BallotWithNoVotes_IsNotReturned()
            {
                var pollManageGuid = new Guid("992E252C-56DB-4E9D-9A31-F5BBA3FA5361");

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = pollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = new Guid("96ADD6EF-DFE4-4160-84C5-63EDC3076A1B")
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();

                poll.Ballots.Add(ballot);

                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageController controller = CreateManageController(contextFactory);


                ManagePollRequestResponseModel response = controller.Get(pollManageGuid);


                Assert.AreEqual(0, response.VotersCount);
            }

            public static ManageController CreateManageController(IContextFactory contextFactory)
            {
                return new ManageController(contextFactory, null)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }
        }
    }
}
