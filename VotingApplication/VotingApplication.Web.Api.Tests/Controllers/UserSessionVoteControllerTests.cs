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
    public class UserSessionVoteControllerTests
    {
        private UserSessionVoteController _controller;
        private Vote _bobVote;

        [TestInitialize]
        public void setup()
        {
            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            InMemoryDbSet<Vote> dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Session> dummySessions = new InMemoryDbSet<Session>(true);

            Session mainSession = new Session() { Id = 1 };
            Session otherSession = new Session() { Id = 2 };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };

            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, SessionId = 1 };
            Vote joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, SessionId = 1 };
            Vote otherVote = new Vote() { Id = 3, OptionId = 1, UserId = 1, SessionId = 2 };

            dummyUsers.Add(bobUser);
            dummyUsers.Add(joeUser);

            dummyVotes.Add(_bobVote);
            dummyVotes.Add(joeVote);
            dummyVotes.Add(otherVote);

            dummyOptions.Add(burgerOption);

            dummySessions.Add(mainSession);
            dummySessions.Add(otherSession);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(dummyVotes);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Sessions).Returns(dummySessions);

            _controller = new UserSessionVoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get(1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsListOfVotesForUserAndSession()
        {
            // Act
            var response = _controller.Get(1, 1);

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
            var response = _controller.Get(99, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetNonexistentSessionIsNotFound()
        {
            // Act
            var response = _controller.Get(1, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetForValidUserAndSessionWithNoVotesIsEmpty()
        {
            // Act
            var response = _controller.Get(2, 2);

            // Assert
            List<Vote> actualVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            List<Vote> expectedVotes = new List<Vote>();
            CollectionAssert.AreEquivalent(expectedVotes, actualVotes);
        }

        [TestMethod]
        public void GetByIdIsAllowed()
        {
            // Act
            var response = _controller.Get(1, 1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public void GetByIdForNonexistentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(99, 1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetByIdForNonexistentSessionIsNotFound()
        {
            // Act
            var response = _controller.Get(1, 99, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetByIdForNonexistentVoteIsNotFound()
        {
            // Act
            var response = _controller.Get(1, 1, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetVoteForDifferentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(1, 1, 2); // Vote 2 belongs to User 2

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 2 does not exist", error.Message);
        }

        [TestMethod]
        public void GetVoteForDifferentSessionIsNotFound()
        {
            // Act
            var response = _controller.Get(1, 1, 3); // Vote 3 belongs to Session 2

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 3 does not exist", error.Message);
        }

        [TestMethod]
        public void GetByIdFetchesIdForThatUserSession()
        {
            // Act
            var response = _controller.Get(1, 1, 1);

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
            var response = _controller.Put(1, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, 1, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, 1, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1, 1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
