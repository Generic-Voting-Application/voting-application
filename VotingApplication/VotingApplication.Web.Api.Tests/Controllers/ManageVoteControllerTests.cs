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
using VotingApplication.Web.Api.Models.DBViewModels;

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
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            Guid mainUUID = Guid.NewGuid();
            Guid otherUUID = Guid.NewGuid();
            Guid emptyUUID = Guid.NewGuid();

            _manageMainUUID = Guid.NewGuid();
            _manageOtherUUID = Guid.NewGuid();
            _manageEmptyUUID = Guid.NewGuid();

            Poll mainPoll = new Poll() { UUID = mainUUID, ManageId = _manageMainUUID };
            Poll otherPoll = new Poll() { UUID = otherUUID, ManageId = _manageOtherUUID };
            Poll emptyPoll = new Poll() { UUID = emptyUUID, ManageId = _manageEmptyUUID };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };

            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, PollId = mainUUID, User = bobUser, Poll = mainPoll, Option = burgerOption };
            _joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, PollId = mainUUID, User = joeUser, Poll = mainPoll, Option = burgerOption };
            _otherVote = new Vote() { Id = 3, OptionId = 1, UserId = 1, PollId = otherUUID };

            dummyUsers.Add(bobUser);
            dummyUsers.Add(joeUser);

            _dummyVotes.Add(_bobVote);
            _dummyVotes.Add(_joeVote);
            _dummyVotes.Add(_otherVote);

            dummyOptions.Add(burgerOption);

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(emptyPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
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

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void GetReturnsVotesForThatPoll()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(2, responseVotes.Count);
            CollectionAssert.AreEqual(new long[] { 1, 2 }, responseVotes.Select(r => r.UserId).ToArray());
            CollectionAssert.AreEqual(new long[] { 1, 1 }, responseVotes.Select(r => r.OptionId).ToArray());
        }

        [TestMethod]
        public void GetOnEmptyPollReturnsEmptyList()
        {
            // Act
            var response = _controller.Get(_manageEmptyUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(0, responseVotes.Count);
        }

        [TestMethod]
        public void GetByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Get(_manageMainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_manageMainUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_manageMainUUID, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_manageMainUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsAllowed()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteFromPollWithNoVotesIsAllowed()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteFromMissingPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Delete(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void DeleteOnlyRemovesVotesFromMatchingPoll()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteByIdIsAllowed()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdRemovesMatchingVote()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID, 2);

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
            var response = _controller.Delete(_manageMainUUID, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdOnMissingVoteDoesNotModifyVotes()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID, 99);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteByIdOnMissingPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Delete(newGuid, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void DeleteByIdOnVoteInOtherPollIsAllowed()
        {
            // Act
            var response = _controller.Delete(_manageEmptyUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdOnVoteInOtherPollDoesNotRemoveOtherVote()
        {
            // Act
            var response = _controller.Delete(_manageEmptyUUID, 1);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }


        #endregion

    }
}
