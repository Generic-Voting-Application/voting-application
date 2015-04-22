using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class TokenPollVoteControllerTests
    {
        private PollVoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;
        private InMemoryDbSet<Vote> _dummyVotes;
        private Guid _mainUUID;
        private Guid _otherUUID;
        private Guid _pointsUUID;
        private Guid _tokenUUID;
        private Guid _timedUUID;
        private Ballot _validBallot;

        private Ballot _bobBallot;
        private Ballot _joeBallot;
        private Ballot _otherBallot;

        private Mock<IVoteValidatorFactory> _mockValidatorFactory;
        private Mock<IMetricEventHandler> _mockMetricHandler;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            _pointsUUID = Guid.NewGuid();
            _tokenUUID = Guid.NewGuid();
            _timedUUID = Guid.NewGuid();
            _validBallot = new Ballot { TokenGuid = Guid.NewGuid() };

            _bobBallot = new Ballot { TokenGuid = Guid.NewGuid() };
            _joeBallot = new Ballot { TokenGuid = Guid.NewGuid() };
            _otherBallot = new Ballot { TokenGuid = Guid.NewGuid() };

            Poll mainPoll = new Poll() { UUID = _mainUUID, ExpiryDate = DateTime.Now.AddMinutes(30), Ballots = new List<Ballot>() { _bobBallot, _joeBallot, _otherBallot } };
            Poll otherPoll = new Poll() { UUID = _otherUUID, Ballots = new List<Ballot>() { _otherBallot } };
            Poll pointsPoll = new Poll() { UUID = _pointsUUID, PollType = PollType.Points, MaxPerVote = 5, MaxPoints = 3, Ballots = new List<Ballot>() { _otherBallot } };
            Poll tokenPoll = new Poll() { UUID = _tokenUUID, Ballots = new List<Ballot>() { _validBallot }, InviteOnly = true };
            Poll timedPoll = new Poll() { UUID = _timedUUID, ExpiryDate = DateTime.Now.AddMinutes(-30) };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            Option pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            Option otherOption = new Option { Id = 3, Name = "Other" };

            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            dummyOptions.Add(burgerOption);
            dummyOptions.Add(pizzaOption);
            dummyOptions.Add(otherOption);

            mainPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            otherPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            pointsPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            tokenPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            timedPoll.Options = new List<Option>() { burgerOption, pizzaOption };

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(pointsPoll);
            dummyPolls.Add(tokenPoll);
            dummyPolls.Add(timedPoll);

            _bobVote = new Vote() { Id = 1, Ballot = _bobBallot, Option = burgerOption, Poll = mainPoll };
            _dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, Poll = mainPoll, Option = new Option() { Id = 1 }, Ballot = _joeBallot };
            _dummyVotes.Add(_joeVote);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            var mockValidator = new Mock<IVoteValidator>();

            _mockValidatorFactory = new Mock<IVoteValidatorFactory>();
            _mockValidatorFactory.Setup(a => a.CreateValidator(PollType.Basic)).Returns(mockValidator.Object);

            _mockMetricHandler = new Mock<IMetricEventHandler>();

            _controller = new PollVoteController(mockContextFactory.Object, _mockMetricHandler.Object, _mockValidatorFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyVotes.Local.Count; i++)
            {
                _dummyVotes.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            _controller.Get(_mainUUID, _bobBallot.TokenGuid);
        }

        [TestMethod]
        public void GetReturnsListOfVotesForUserAndPoll()
        {
            // Act
            var response = _controller.Get(_mainUUID, _bobBallot.TokenGuid);

            // Assert
            Assert.AreEqual(1, response.Count);
            Assert.AreEqual(1, response.Single().OptionId);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Get(newGuid, _bobBallot.TokenGuid);

        }

        [TestMethod]
        public void GetForValidUserAndPollWithNoVotesIsEmpty()
        {
            // Act
            var response = _controller.Get(_otherUUID, _bobBallot.TokenGuid);

            // Assert
            Assert.AreEqual(0, response.Count);
        }

        #endregion

        #region PUT

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostRejectsVoteWithInvalidInput()
        {
            // Arrange
            _controller.ModelState.AddModelError("OptionId", "");

            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new List<VoteRequestModel>());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutNonexistentOptionIsNotAllowed()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 99 } });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PutNonexistentPollIsNotAllowed()
        {
            // Act
            _controller.Put(Guid.NewGuid(), _bobBallot.TokenGuid, new List<VoteRequestModel>() { });
        }

        [TestMethod]
        public void PutWithNewVoteIsAllowed()
        {
            // Act
            _controller.Put(_mainUUID, _otherBallot.TokenGuid, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1, VoteValue = 0 } });

            // Assert
            Assert.AreEqual(3, _dummyVotes.Local.Count);
        }

        [TestMethod]
        public void PutReplacesCurrentVote()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1, VoteValue = 0 } });

            // Assert
            Assert.AreEqual(2, _dummyVotes.Local.Count);
        }

        [TestMethod]
        public void PutWithNewVoteSetsVoteValueCorrectly()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1, VoteValue = 0 } });

            // Assert
            Assert.AreEqual(0, _dummyVotes.Local.Single(v => v.Ballot.TokenGuid == _bobBallot.TokenGuid && v.Poll.UUID == _mainUUID).VoteValue);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutInvalidOptionIsNotAllowed()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 3 } });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
        public void PutOnAnExpiredPollNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { Option = new Option() { Id = 1 }, Poll = new Poll() { UUID = _timedUUID } };

            // Act
            _controller.Put(_timedUUID, _bobBallot.TokenGuid, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1 } });
        }

        [TestMethod]
        public void PutSelectsCorrectValidatorForTypeOfPoll()
        {
            // Arrange
            var pointsValidator = new Mock<IVoteValidator>();
            _mockValidatorFactory.Setup(a => a.CreateValidator(PollType.Points)).Returns(pointsValidator.Object);

            // Act
            _controller.Put(_pointsUUID, _otherBallot.TokenGuid, new List<VoteRequestModel> { new VoteRequestModel { OptionId = 1, VoteValue = 2 } });

            // Assert
            pointsValidator.Verify(a => a.Validate(It.IsAny<List<VoteRequestModel>>(), It.IsAny<Poll>(), It.IsAny<ModelStateDictionary>()));
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
        public void PutOnInviteOnlyPollWithUnrecognisedTokenIsForbidden()
        {
            // Arrange
            var voteRequests = new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1 } };

            // Act
            _controller.Put(_tokenUUID, Guid.NewGuid(), voteRequests);
        }

        [TestMethod]
        public void PutOnOpenPollWithUnrecognisedTokenIsAllowed()
        {
            // Arrange
            var voteRequests = new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1 } };

            // Act
            _controller.Put(_mainUUID, Guid.NewGuid(), voteRequests);
        }

        [TestMethod]
        public void PutNewVoteGeneratesAddVoteMetric()
        {
            // Arrange
            var voteRequests = new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1 } };

            // Act
            _controller.Put(_mainUUID, Guid.NewGuid(), voteRequests);

            // Assert
            _mockMetricHandler.Verify(m => m.VoteAddedEvent(It.Is<Vote>(v => v.Option.Id == 1), _mainUUID), Times.Once());
        }

        [TestMethod]
        public void ClearingVoteGeneratesRemoveVoteMetric()
        {
            // Arrange
            Guid tokenGuid = Guid.NewGuid();
            var existingVote = new Vote() { Option = new Option() { Id = 1 }, Poll = new Poll() { UUID = _mainUUID }, Ballot = new Ballot() { TokenGuid = tokenGuid } };
            _dummyVotes.Add(existingVote);
            var voteRequests = new List<VoteRequestModel>();

            // Act
            _controller.Put(_mainUUID, tokenGuid, voteRequests);

            // Assert
            _mockMetricHandler.Verify(m => m.VoteDeletedEvent(existingVote, _mainUUID), Times.Once());
        }

        #endregion
    }
}
