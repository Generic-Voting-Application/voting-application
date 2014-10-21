using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
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
        private Vote _testVote;

        [TestInitialize]
        public void setup()
        {
            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            User bobUser = new User { Id = 1, Name = "Bob" };

            _testVote = new Vote() { Id = 1, Option = burgerOption, User = bobUser };
            List<Vote> dummyVotes = new List<Vote>();
            dummyVotes.Add(_testVote);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(dummyVotes);

            _controller = new VoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

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
            expectedVotes.Add(_testVote);
            CollectionAssert.AreEquivalent(expectedVotes, responseVotes);
        }
    }
}
