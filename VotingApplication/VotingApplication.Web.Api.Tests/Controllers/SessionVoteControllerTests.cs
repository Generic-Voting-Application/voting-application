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
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Session> dummySessions = new InMemoryDbSet<Session>(true);

            Session mainSession = new Session() { Id = 1 };
            Session otherSession = new Session() { Id = 2 };
            Session emptySession = new Session() { Id = 3 };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };

            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, SessionId = 1 };
            _joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, SessionId = 1 };
            Vote otherVote = new Vote() { Id = 3, OptionId = 1, UserId = 1, SessionId = 2 };

            dummyUsers.Add(bobUser);
            dummyUsers.Add(joeUser);

            _dummyVotes.Add(_bobVote);
            _dummyVotes.Add(_joeVote);
            _dummyVotes.Add(otherVote);

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
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetNonexistentSessionIsNotFound()
        {
            // Act
            var response = _controller.Get(99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetReturnsVotesForThatSession()
        {
            // Act
            var response = _controller.Get(1);

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
            var response = _controller.Get(3);

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            List<Vote> responseVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            CollectionAssert.AreEquivalent(expectedVotes, responseVotes);
        }

        [TestMethod]
        public void GetByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Get(1, 1);

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
            var response = _controller.Put(1, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
