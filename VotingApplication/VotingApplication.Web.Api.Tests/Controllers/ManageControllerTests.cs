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
    public class ManageControllerTests
    {
        private ManageController _controller;
        private Guid _manageMainUUID;
        private Guid _manageEmptyUUID;
        private Option _burgerOption;
        private Option _pizzaOption;
        private Vote _burgerVote;
        private Poll _mainPoll;
        private InMemoryDbSet<Option> _dummyOptions;
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

            _burgerOption = new Option { Id = 1, Name = "Burger King" };
            _pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            _dummyOptions = new InMemoryDbSet<Option>(true);
            _dummyOptions.Add(_burgerOption);
            _dummyOptions.Add(_pizzaOption);

            _burgerVote = new Vote() { Id = 1, Poll = new Poll() { UUID = mainUUID }, Option = new Option() { Id = 1 } };
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            _dummyVotes.Add(_burgerVote);

            _dummyPolls = new InMemoryDbSet<Poll>(true);
            _mainPoll = new Poll() { UUID = mainUUID, ManageId = _manageMainUUID, Options = new List<Option>() { _burgerOption, _pizzaOption }, Ballots = new List<Ballot>() };
            Poll emptyPoll = new Poll() { UUID = emptyUUID, ManageId = _manageEmptyUUID, Options = new List<Option>(), Ballots = new List<Ballot>() };
            _dummyPolls.Add(_mainPoll);
            _dummyPolls.Add(emptyPoll);

            _dummyTokens = new InMemoryDbSet<Ballot>(true);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Options).Returns(_dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Ballots).Returns(_dummyTokens);

            _controller = new ManageController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyOptions.Count(); i++)
            {
                _dummyOptions.Local[i].Id = (long)i + 1;
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
        public void GetWithEmptyPollReturnsEmptyOptionList()
        {
            // Act
            var response = _controller.Get(_manageEmptyUUID);

            // Assert
            Assert.AreEqual(0, response.Options.Count);
        }

        [TestMethod]
        public void GetWithPopulatedPollReturnsOptionsForThatPoll()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            Assert.AreEqual(2, response.Options.Count);
            CollectionAssert.AreEqual(new string[] { "Burger King", "Pizza Hut" }, response.Options.Select(r => r.Name).ToArray());
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

        #region PUT

        [TestMethod]
        public void PutIsAllowed()
        {
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Voters = new List<ManageInvitationRequestModel>()
            };

            // Act
            _controller.Put(_manageMainUUID, request);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PutReturnsNotFoundForMissingPoll()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Put(newGuid, new ManagePollUpdateRequest());
        }

        [TestMethod]
        public void PutOverwritesExistingOptionsOnAPoll()
        {
            List<Option> newOptions = new List<Option>() { new Option() { Name = "Test", Description = "Abc" } };
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Options = newOptions,
                Voters = new List<ManageInvitationRequestModel>()
            };
            _controller.Put(_manageMainUUID, request);

            // Assert
            CollectionAssert.AreEquivalent(_mainPoll.Options, newOptions);
            Assert.AreEqual(0, _mainPoll.Options[0].Id);
        }

        [TestMethod]
        public void PutRetainsExistingOptionsOnAPollIfIdsMatch()
        {
            List<Option> newOptions = new List<Option>() { new Option() { Name = "Test", Description = "Abc" }, _burgerOption };
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Options = newOptions,
                Voters = new List<ManageInvitationRequestModel>()
            };
            _controller.Put(_manageMainUUID, request);

            // Assert
            Assert.AreEqual(_burgerOption, _mainPoll.Options.Find(o => o.Id == _burgerOption.Id));
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutInvalidOptionIsRejected()
        {
            // Act
            List<Option> invalidOptions = new List<Option>() { new Option() { Description = "Abc" } };
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Options = invalidOptions,
                Voters = new List<ManageInvitationRequestModel>()
            };
            _controller.Put(_manageMainUUID, request);
        }

        [TestMethod]
        public void PutClearsOptionsIfNoneAreGiven()
        {
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Voters = new List<ManageInvitationRequestModel>()
            };
            _controller.Put(_manageMainUUID, request);

            // Assert
            Assert.AreEqual(0, _mainPoll.Options.Count);
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
                    Option = new Option()
                    {
                        PollOptionNumber = 1,

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
                return new ManageController(contextFactory)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }
        }
    }
}
