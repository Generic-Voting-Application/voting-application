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
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Tests.TestHelpers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class ManageVoteControllerTests
    {
        private ManageVoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;
        private Vote _otherVote;
        private Guid _manageMainUUID;
        private Guid _manageOtherUUID;
        private Guid _manageEmptyUUID;
        private Poll _mainPoll;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            Guid mainUUID = Guid.NewGuid();
            Guid otherUUID = Guid.NewGuid();
            Guid emptyUUID = Guid.NewGuid();

            _manageMainUUID = Guid.NewGuid();
            _manageOtherUUID = Guid.NewGuid();
            _manageEmptyUUID = Guid.NewGuid();

            _mainPoll = new Poll() { UUID = mainUUID, ManageId = _manageMainUUID, LastUpdated = new DateTime(100) };
            Poll otherPoll = new Poll() { UUID = otherUUID, ManageId = _manageOtherUUID };
            Poll emptyPoll = new Poll() { UUID = emptyUUID, ManageId = _manageEmptyUUID };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };

            _bobVote = new Vote() { Id = 1, Poll = _mainPoll, Option = burgerOption, Ballot = new Ballot() };
            _joeVote = new Vote() { Id = 2, Poll = _mainPoll, Option = burgerOption, Ballot = new Ballot() };
            _otherVote = new Vote() { Id = 3, Poll = new Poll() { UUID = otherUUID }, Option = new Option() { Id = 1 }, Ballot = new Ballot() };

            _dummyVotes.Add(_bobVote);
            _dummyVotes.Add(_joeVote);
            _dummyVotes.Add(_otherVote);

            dummyOptions.Add(burgerOption);

            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(emptyPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);

            _controller = new ManageVoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region DELETE

        [TestMethod]
        public void DeleteOnlyRemovesVotesFromMatchingPoll()
        {
            // Act
            _controller.Delete(_manageMainUUID);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }


        [TestMethod]
        public void DeleteByIdIsAllowed()
        {
            // Act
            _controller.Delete(_manageMainUUID, 1);
        }

        [TestMethod]
        public void DeleteByIdRemovesMatchingVote()
        {
            // Act
            _controller.Delete(_manageMainUUID, 2);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteByIdOnMissingVoteIsAllowed()
        {
            // Act
            _controller.Delete(_manageMainUUID, 99);
        }

        [TestMethod]
        public void DeleteByIdOnMissingVoteDoesNotModifyVotes()
        {
            // Act
            _controller.Delete(_manageMainUUID, 99);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void DeleteByIdOnMissingPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Delete(newGuid, 1);
        }

        [TestMethod]
        public void DeleteByIdOnVoteInOtherPollIsAllowed()
        {
            // Act
            _controller.Delete(_manageEmptyUUID, 1);
        }

        [TestMethod]
        public void DeleteByIdOnVoteInOtherPollDoesNotRemoveOtherVote()
        {
            // Act
            _controller.Delete(_manageEmptyUUID, 1);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteSingleVoteUpdatesLastUpdatedTime()
        {
            // Act
            _controller.Delete(_manageMainUUID, 1);

            // Assert
            Assert.AreNotEqual(new DateTime(100), _mainPoll.LastUpdated);
        }

        #endregion

        public readonly Guid PollManageGuid = new Guid("EBDE4ED9-D014-4145-B998-13B5A247BB4B");

        [TestClass]
        public class GetTests : ManageVoteControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);

                ManageVoteController controller = CreateManageVoteController(contextFactory);

                controller.Get(PollManageGuid);
            }

            [TestMethod]
            public void PollWithNoBallots_ReturnsEmptyList()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();

                var poll = new Poll()
                 {
                     ManageId = PollManageGuid
                 };

                existingPolls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);

                ManageVoteController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(PollManageGuid);


                CollectionAssert.AreEqual(new List<ManageVoteResponseModel>(), response);
            }

            [TestMethod]
            public void PollWithBallotContainingNoVotes_ReturnsEmptyList()
            {
                Guid pollManageGuid = new Guid("EBDE4ED9-D014-4145-B998-13B5A247BB4B");

                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll =
                    new Poll()
                    {
                        ManageId = pollManageGuid,
                        Ballots = new List<Ballot>()
                        {
                            new Ballot()
                        }
                    };

                existingPolls.Add(poll);
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);

                ManageVoteController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(pollManageGuid);


                CollectionAssert.AreEqual(new List<ManageVoteResponseModel>(), response);
            }

            [TestMethod]
            public void PollWithBallotsWithVotes_ReturnsAll()
            {
                var manageGuid = new Guid("A76287F6-BC56-421C-9294-A477D1E9C4B3");
                const string voterName = "Derek";
                const string optionName = "Value?";
                const int optionValue = 23;

                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                var ballot = new Ballot()
                {
                    ManageGuid = manageGuid,
                    VoterName = voterName,
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var vote = new Vote()
                {
                    Option = new Option()
                    {
                        Name = optionName
                    },
                    VoteValue = optionValue
                };


                ballot.Votes.Add(vote);
                poll.Ballots.Add(ballot);
                existingPolls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManageVoteController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(PollManageGuid);


                Assert.AreEqual(1, response.Count);

                ManageVoteResponseModel responseBallot = response[0];

                Assert.AreEqual(manageGuid, responseBallot.BallotManageGuid);
                Assert.AreEqual(voterName, responseBallot.VoterName);
                Assert.AreEqual(1, responseBallot.Votes.Count);

                VoteResponse responseVote = responseBallot.Votes[0];

                Assert.AreEqual(optionName, responseVote.OptionName);
                Assert.AreEqual(optionValue, responseVote.Value);
            }

            [TestMethod]
            public void MultiplePolls_ReturnsOnlyRequested()
            {
                Guid otherManageGuid = new Guid("35FC553F-2F0E-49E0-A919-802900262046");

                const string expectedVoterName = "Expected Voter";
                const string otherVoterName = "Someone else voting";


                const string expectedOptionName = "SomeOption";
                var expectedOption = new Option() { Name = expectedOptionName };
                const int expectedVoteValue = 42;

                var otherOption = new Option() { Name = "Some Other Option" };
                const int otherVoteValue = 16;


                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();

                var poll1 = new Poll()
                {
                    ManageId = PollManageGuid,
                    Ballots = new List<Ballot>()
                    {
                        CreateBallot(expectedVoterName, CreateVote(expectedOption, expectedVoteValue))
                    }
                };
                var poll2 =
                    new Poll()
                    {
                        ManageId = otherManageGuid,
                        Ballots = new List<Ballot>()
                        {
                            CreateBallot(otherVoterName, CreateVote(otherOption, otherVoteValue))
                        }
                    };
                existingPolls.Add(poll1);
                existingPolls.Add(poll2);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManageVoteController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(PollManageGuid);

                Assert.AreEqual(1, response.Count);

                ManageVoteResponseModel responseBallot = response[0];

                Assert.AreEqual(expectedVoterName, responseBallot.VoterName);
                Assert.AreEqual(1, responseBallot.Votes.Count);

                VoteResponse responseVote = responseBallot.Votes[0];

                Assert.AreEqual(expectedOptionName, responseVote.OptionName);
                Assert.AreEqual(42, responseVote.Value);
            }
        }

        [TestClass]
        public class DeleteTests : ManageVoteControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void NoMatchingPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoteController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);
            }

            [TestMethod]
            public void BallotsAreRemovedFromPoll()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot()
                {
                    VoterName = "Derek",
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var ballot2 = new Ballot()
                {
                    VoterName = "Mavis",
                    TokenGuid = new Guid("C4865232-2A1A-4BB5-8EF7-5F6C17CFAC8A")
                };


                poll.Ballots.Add(ballot1);
                poll.Ballots.Add(ballot2);

                ballots.Add(ballot1);
                ballots.Add(ballot2);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                ManageVoteController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);

                Assert.AreEqual(0, ballots.Count());
            }

            [TestMethod]
            public void VotesAreRemovedFromBallots()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot()
                {
                    VoterName = "Derek",
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var ballot2 = new Ballot()
                {
                    VoterName = "Mavis",
                    TokenGuid = new Guid("C4865232-2A1A-4BB5-8EF7-5F6C17CFAC8A")
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote();
                var vote2 = new Vote();
                var vote3 = new Vote();
                var vote4 = new Vote();



                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);
                ballot2.Votes.Add(vote3);
                ballot2.Votes.Add(vote4);

                poll.Ballots.Add(ballot1);
                poll.Ballots.Add(ballot2);

                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);
                votes.Add(vote4);

                ballots.Add(ballot1);
                ballots.Add(ballot2);

                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoteController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);

                Assert.AreEqual(0, ballots.Count());
                Assert.AreEqual(0, votes.Count());
            }

            [TestMethod]
            public void MultiplePolls_OnlyAffectsRequested()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll1 = new Poll()
                {
                    ManageId = PollManageGuid
                };
                var poll2 = new Poll()
                {
                    ManageId = new Guid("600D6D95-2C77-483E-B708-5946B848161D")
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot()
                {
                    VoterName = "Derek",
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var ballot2 = new Ballot()
                {
                    VoterName = "Mavis",
                    TokenGuid = new Guid("C4865232-2A1A-4BB5-8EF7-5F6C17CFAC8A")
                };

                var ballot3 = new Ballot()
                {
                    VoterName = "Boris",
                    TokenGuid = new Guid("606E358F-B1BE-445A-AE93-2E5F5A824D67")
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Id = 1 };
                var vote2 = new Vote() { Id = 2 };
                var vote3 = new Vote() { Id = 3 };
                var vote4 = new Vote() { Id = 4 };
                var vote5 = new Vote() { Id = 5 };
                var vote6 = new Vote() { Id = 6 };



                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);
                ballot2.Votes.Add(vote3);
                ballot2.Votes.Add(vote4);
                ballot3.Votes.Add(vote5);
                ballot3.Votes.Add(vote6);

                poll1.Ballots.Add(ballot1);
                poll1.Ballots.Add(ballot2);

                poll2.Ballots.Add(ballot3);

                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);
                votes.Add(vote4);
                votes.Add(vote5);
                votes.Add(vote6);

                ballots.Add(ballot1);
                ballots.Add(ballot2);
                ballots.Add(ballot3);

                polls.Add(poll1);
                polls.Add(poll2);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoteController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);

                Assert.AreEqual(1, ballots.Count());
                Assert.AreEqual(ballot3, ballots.Single());

                Assert.AreEqual(2, votes.Count());
                Assert.AreEqual(5, votes.First().Id);
                Assert.AreEqual(6, votes.Last().Id);
            }
        }

        public static ManageVoteController CreateManageVoteController(IContextFactory contextFactory)
        {
            return new ManageVoteController(contextFactory)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        public Vote CreateVote(Option option, int value)
        {
            return new Vote() { Option = option, VoteValue = value };
        }

        public Ballot CreateBallot(string voterName, Vote vote)
        {
            return new Ballot() { VoterName = voterName, Votes = new List<Vote>() { vote } };
        }
    }
}
