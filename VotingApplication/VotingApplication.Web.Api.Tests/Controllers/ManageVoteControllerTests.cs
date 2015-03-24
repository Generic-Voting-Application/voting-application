using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;

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

            _bobVote = new Vote() { Id = 1, Poll = _mainPoll, Option = burgerOption, Token = new Token() };
            _joeVote = new Vote() { Id = 2, Poll = _mainPoll, Option = burgerOption, Token = new Token() };
            _otherVote = new Vote() { Id = 3, Poll = new Poll() { UUID = otherUUID }, Option = new Option() { Id = 1 }, Token = new Token() };

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

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();

            _controller.Get(newGuid);
        }

        [TestMethod]
        public void GetReturnsVotesForThatPoll()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            Assert.AreEqual(2, response.Count);
            CollectionAssert.AreEqual(new long[] { 1, 1 }, response.Select(r => r.OptionId).ToArray());
        }

        [TestMethod]
        public void GetOnEmptyPollReturnsEmptyList()
        {
            // Act
            var response = _controller.Get(_manageEmptyUUID);

            // Assert
            Assert.AreEqual(0, response.Count);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsAllowed()
        {
            // Act
            _controller.Delete(_manageMainUUID);
        }

        [TestMethod]
        public void DeleteFromPollWithNoVotesIsAllowed()
        {
            // Act
            _controller.Delete(_manageMainUUID);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void DeleteFromMissingPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Delete(newGuid);
        }

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
        public void DeleteAllVotesUpdatesLastUpdatedTime()
        {
            // Act
            _controller.Delete(_manageMainUUID);

            // Assert
            Assert.AreNotEqual(new DateTime(100), _mainPoll.LastUpdated);
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

    }
}
