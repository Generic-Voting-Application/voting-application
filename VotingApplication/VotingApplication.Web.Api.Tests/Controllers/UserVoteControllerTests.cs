using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class UserVoteControllerTests
    {
        private UserVoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;

        [TestInitialize]
        public void setup()
        {
            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            InMemoryDbSet<Vote> dummyVotes = new InMemoryDbSet<Vote>(true);

            _bobVote = new Vote() { Id = 1, Option = burgerOption, User = bobUser, UserId = 1 };
            dummyUsers.Add(bobUser);
            dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, Option = burgerOption, User = joeUser, UserId = 2 };
            dummyUsers.Add(joeUser);
            dummyVotes.Add(_joeVote);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(dummyVotes);
            mockContext.Setup(b => b.Users).Returns(dummyUsers);

            _controller = new UserVoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetByUserIdReturnsVotes()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetByUserIdReturnsVotesForThatUser()
        {
            // Act
            var response = _controller.Get(1);
            List<Vote> responseVotes = ((ObjectContent)response.Content).Value as List<Vote>;

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            CollectionAssert.AreEquivalent(expectedVotes, responseVotes);
        }

        [TestMethod]
        public void GetByUserIdReturns404ForUnknownUser()
        {
            // Act
            var response = _controller.Get(3);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 3 not found", error.Message);
        }

        [TestMethod]
        public void GetVoteForUserByVoteIdReturnsVote()
        {
            // Act
            var response = _controller.Get(1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetVoteForUserByVoteIdReturnsVoteMatchingId()
        {
            // Act
            var response = _controller.Get(2, 2);
            Vote responseVote = ((ObjectContent)response.Content).Value as Vote;

            // Assert
            Assert.AreEqual(_joeVote, responseVote);
        }

        [TestMethod]
        public void GetVoteForUserByVoteIdReturns404ForWrongUser()
        {
            // Act
            var response = _controller.Get(1, 2);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 2 not found", error.Message);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
