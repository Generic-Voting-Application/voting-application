using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class UserPollVoteControllerTests
    {
        private UserPollVoteController _controller;
        private Vote _bobVote;
        private Guid _mainPollUUID;
        private Guid _otherPollUUID;

        [TestInitialize]
        public void setup()
        {
            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            InMemoryDbSet<Vote> dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            _mainPollUUID = Guid.NewGuid();
            _otherPollUUID = Guid.NewGuid();
            Poll mainPoll = new Poll() { UUID = _mainPollUUID };
            Poll otherPoll = new Poll() { UUID = _otherPollUUID };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };

            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, PollId = _mainPollUUID };
            Vote joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, PollId = _mainPollUUID };
            Vote otherVote = new Vote() { Id = 3, OptionId = 1, UserId = 1, PollId = _otherPollUUID };

            dummyUsers.Add(bobUser);
            dummyUsers.Add(joeUser);

            dummyVotes.Add(_bobVote);
            dummyVotes.Add(joeVote);
            dummyVotes.Add(otherVote);

            dummyOptions.Add(burgerOption);

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(dummyVotes);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);

            _controller = new UserPollVoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get(1, _mainPollUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsListOfVotesForUserAndPoll()
        {
            // Act
            var response = _controller.Get(1, _mainPollUUID);

            // Assert
            List<Vote> actualVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            CollectionAssert.AreEquivalent(expectedVotes, actualVotes);
        }

        [TestMethod]
        public void GetNonexistentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(99, _mainPollUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(1, newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void GetForValidUserAndPollWithNoVotesIsEmpty()
        {
            // Act
            var response = _controller.Get(2, _otherPollUUID);

            // Assert
            List<Vote> actualVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            List<Vote> expectedVotes = new List<Vote>();
            CollectionAssert.AreEquivalent(expectedVotes, actualVotes);
        }

        [TestMethod]
        public void GetByIdIsAllowed()
        {
            // Act
            var response = _controller.Get(1, _mainPollUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public void GetByIdForNonexistentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(99, _mainPollUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetByIdForNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(1, newGuid, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void GetByIdForNonexistentVoteIsNotFound()
        {
            // Act
            var response = _controller.Get(1, _mainPollUUID, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetVoteForDifferentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(1, _mainPollUUID, 2); // Vote 2 belongs to User 2

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 2 does not exist", error.Message);
        }

        [TestMethod]
        public void GetVoteForDifferentPollIsNotFound()
        {
            // Act
            var response = _controller.Get(1, _mainPollUUID, 3); // Vote 3 belongs to Poll 2

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 3 does not exist", error.Message);
        }

        [TestMethod]
        public void GetByIdFetchesIdForThatUserPoll()
        {
            // Act
            var response = _controller.Get(1, _mainPollUUID, 1);

            // Assert
            var responseVote = ((ObjectContent)response.Content).Value as Vote;
            Assert.AreEqual(_bobVote, responseVote);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, _mainPollUUID, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, _mainPollUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, _mainPollUUID, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, _mainPollUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1, _mainPollUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1, _mainPollUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
