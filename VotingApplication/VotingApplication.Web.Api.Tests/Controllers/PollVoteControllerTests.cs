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
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class PollVoteControllerTests
    {
        private PollVoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;
        private Vote _otherVote;
        private Vote _anonymousVote;
        private Guid _mainUUID;
        private Guid _otherUUID;
        private Guid _emptyUUID;
        private Guid _anonymousUUID;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();
            _anonymousUUID = Guid.NewGuid();

            Poll mainPoll = new Poll() { UUID = _mainUUID, LastUpdated = DateTime.Today };
            Poll otherPoll = new Poll() { UUID = _otherUUID };
            Poll emptyPoll = new Poll() { UUID = _emptyUUID };
            Poll anonymousPoll = new Poll() { UUID = _anonymousUUID };

            anonymousPoll.AnonymousVoting = true;

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };

            _bobVote = new Vote() { Id = 1, OptionId = 1, PollId = _mainUUID, Poll = mainPoll, Option = burgerOption, Token = new Token() };
            _joeVote = new Vote() { Id = 2, OptionId = 1, PollId = _mainUUID, Poll = mainPoll, Option = burgerOption, Token = new Token() };
            _otherVote = new Vote() { Id = 3, OptionId = 1, PollId = _otherUUID, Token = new Token() };
            _anonymousVote = new Vote() { Id = 4, OptionId = 1, PollId = _anonymousUUID, Token = new Token() };

            _dummyVotes.Add(_bobVote);
            _dummyVotes.Add(_joeVote);
            _dummyVotes.Add(_otherVote);
            _dummyVotes.Add(_anonymousVote);

            dummyOptions.Add(burgerOption);

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(emptyPoll);
            dummyPolls.Add(anonymousPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);

            _controller = new PollVoteController(mockContextFactory.Object);
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
            var response = _controller.Get(_mainUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(2, responseVotes.Count);
            CollectionAssert.AreEqual(new long[] { 1, 1 }, responseVotes.Select(r => r.OptionId).ToArray());
        }

        [TestMethod]
        public void GetOnEmptyPollReturnsEmptyList()
        {
            // Act
            var response = _controller.Get(_emptyUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(0, responseVotes.Count);
        }

        [TestMethod]
        public void GetOnAnonPollDoesNotReturnUsernames()
        {
            // Act
            var response = _controller.Get(_anonymousUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(1, responseVotes.Count);
            Assert.AreEqual("Anonymous User", responseVotes[0].VoterName);
        }

        [TestMethod]
        public void GetWithLowTimestampReturnsResults()
        {
            // Act
            Uri requestURI;
            Uri.TryCreate("http://localhost/?lastPoll=0", UriKind.Absolute, out requestURI);
            _controller.Request.RequestUri = requestURI;
            var response = _controller.Get(_mainUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(2, responseVotes.Count);
        }

        [TestMethod]
        public void GetWithHighTimestampReturnsNotModified()
        {
            // Act
            Uri requestURI;
            Uri.TryCreate("http://localhost/?lastPoll=2145916800000", UriKind.Absolute, out requestURI);
            _controller.Request.RequestUri = requestURI;
            var response = _controller.Get(_mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotModified, response.StatusCode);
        }

        [TestMethod]
        public void GetWithHighTimestampReturnsNoResults()
        {
            // Act
            Uri requestURI;
            Uri.TryCreate("http://localhost/?lastPoll=2145916800000", UriKind.Absolute, out requestURI);
            _controller.Request.RequestUri = requestURI;
            var response = _controller.Get(_mainUUID);

            // Assert
            Assert.IsNull(response.Content);
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
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(_mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
