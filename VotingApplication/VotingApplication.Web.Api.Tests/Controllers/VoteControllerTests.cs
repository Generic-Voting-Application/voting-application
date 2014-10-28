using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;

namespace VotingApplication.Web.Api.Tests
{
    [TestClass]
    public class VoteControllerTests
    {
        private VoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;

        [TestInitialize]
        public void setup()
        {
            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            InMemoryDbSet<Vote> dummyVotes = new InMemoryDbSet<Vote>(true);

            _bobVote = new Vote() { Id = 1, Option = burgerOption, User = bobUser };
            dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, Option = burgerOption, User = joeUser };
            dummyVotes.Add(_joeVote);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(dummyVotes);

            _controller = new VoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetReturnsAllVotes()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsNonNullVotes()
        {
            // Act
            var response = _controller.Get();
            List<Vote> responseVotes = ((ObjectContent)response.Content).Value as List<Vote>;

            // Assert
            Assert.IsNotNull(responseVotes);
        }

        [TestMethod]
        public void GetReturnsVotesFromTheDatabase()
        {
            // Act
            var response = _controller.Get();
            List<Vote> responseVotes = ((ObjectContent)response.Content).Value as List<Vote>;

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            CollectionAssert.AreEquivalent(expectedVotes, responseVotes);
        }

        [TestMethod]
        public void GetWithIdFindsVoteWithId()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsVoteWithIdFromTheDatabase()
        {
            // Act
            var response = _controller.Get(2);
            Vote responseVote = ((ObjectContent)response.Content).Value as Vote;

            // Assert
            Assert.AreEqual(_joeVote.Id, responseVote.Id);
            Assert.AreEqual(_joeVote.UserId, responseVote.UserId);
            Assert.AreEqual(_joeVote.OptionId, responseVote.OptionId);
        }

        [TestMethod]
        public void GetWithIdReturnsErrorCode404ForUnknownVoteID()
        {
            // Act
            var response = _controller.Get(3);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsNullUserForUnknownVoteID()
        {
            // Act
            var response = _controller.Get(3);
            Vote responseVote = ((ObjectContent)response.Content).Value as Vote;

            // Assert
            Assert.IsNull(responseVote);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(new List<object>());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Vote());

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
