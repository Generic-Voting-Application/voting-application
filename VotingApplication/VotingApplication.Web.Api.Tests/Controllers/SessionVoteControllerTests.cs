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
    public class SessionVoteControllerTests
    {
        private SessionVoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;
        private Vote _otherVote;
        private Guid _mainUUID;
        private Guid _otherUUID;
        private Guid _emptyUUID;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Session> dummySessions = new InMemoryDbSet<Session>(true);

            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();

            Session mainSession = new Session() { UUID = _mainUUID };
            Session otherSession = new Session() { UUID = _otherUUID };
            Session emptySession = new Session() { UUID = _emptyUUID };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };

            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, SessionId = _mainUUID };
            _joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, SessionId = _mainUUID };
            _otherVote = new Vote() { Id = 3, OptionId = 1, UserId = 1, SessionId = _otherUUID };

            dummyUsers.Add(bobUser);
            dummyUsers.Add(joeUser);

            _dummyVotes.Add(_bobVote);
            _dummyVotes.Add(_joeVote);
            _dummyVotes.Add(_otherVote);

            dummyOptions.Add(burgerOption);

            dummySessions.Add(mainSession);
            dummySessions.Add(otherSession);
            dummySessions.Add(emptySession);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Sessions).Returns(dummySessions);

            _controller = new SessionVoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetNonexistentSessionIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void GetReturnsVotesForThatSession()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            List<Vote> responseVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            CollectionAssert.AreEquivalent(expectedVotes, responseVotes);
        }

        [TestMethod]
        public void GetOnEmptySessionReturnsEmptyList()
        {
            // Act
            var response = _controller.Get(_emptyUUID);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            List<Vote> responseVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            CollectionAssert.AreEquivalent(expectedVotes, responseVotes);
        }

        [TestMethod]
        public void GetByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Get(_mainUUID, 1);

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
            var response = _controller.Put(_mainUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(_mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsAllowed()
        {
            // Act
            var response = _controller.Delete(_mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdRemovesMatchingVote()
        {
            // Act
            var response = _controller.Delete(_mainUUID, 2);

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
            var response = _controller.Delete(_mainUUID, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdOnMissingVoteDoesNotModifyVotes()
        {
            // Act
            var response = _controller.Delete(_mainUUID, 99);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteByIdOnMissingSessionIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Delete(newGuid, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void DeleteByIdOnVoteInOtherSessionIsAllowed()
        {
            // Act
            var response = _controller.Delete(_emptyUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdOnVoteInOtherSessionDoesNotRemoveOtherVote()
        {
            // Act
            var response = _controller.Delete(_emptyUUID, 1);

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
