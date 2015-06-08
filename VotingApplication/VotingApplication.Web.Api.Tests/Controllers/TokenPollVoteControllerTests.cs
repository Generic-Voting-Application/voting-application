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
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web.Tests.Controllers
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

        private Poll _mainPoll;

        private Mock<IVoteValidatorFactory> _mockValidatorFactory;
        private Mock<IMetricHandler> _mockMetricHandler;

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

            _mainPoll = new Poll() { UUID = _mainUUID, ExpiryDateUtc = DateTime.UtcNow.AddMinutes(30), Ballots = new List<Ballot>() { _bobBallot, _joeBallot, _otherBallot } };
            Poll otherPoll = new Poll() { UUID = _otherUUID, Ballots = new List<Ballot>() { _otherBallot } };
            Poll pointsPoll = new Poll() { UUID = _pointsUUID, PollType = PollType.Points, MaxPerVote = 5, MaxPoints = 3, Ballots = new List<Ballot>() { _otherBallot } };
            Poll tokenPoll = new Poll() { UUID = _tokenUUID, Ballots = new List<Ballot>() { _validBallot }, InviteOnly = true };
            Poll timedPoll = new Poll() { UUID = _timedUUID, ExpiryDateUtc = DateTime.UtcNow.AddMinutes(-30) };

            Choice burgerChoice = new Choice { Id = 1, Name = "Burger King" };
            Choice pizzaChoice = new Choice { Id = 2, Name = "Pizza Hut" };
            Choice otherChoice = new Choice { Id = 3, Name = "Other" };

            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Choice> dummyChoices = new InMemoryDbSet<Choice>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            dummyChoices.Add(burgerChoice);
            dummyChoices.Add(pizzaChoice);
            dummyChoices.Add(otherChoice);

            _mainPoll.Choices = new List<Choice>() { burgerChoice, pizzaChoice };
            otherPoll.Choices = new List<Choice>() { burgerChoice, pizzaChoice };
            pointsPoll.Choices = new List<Choice>() { burgerChoice, pizzaChoice };
            tokenPoll.Choices = new List<Choice>() { burgerChoice, pizzaChoice };
            timedPoll.Choices = new List<Choice>() { burgerChoice, pizzaChoice };

            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(pointsPoll);
            dummyPolls.Add(tokenPoll);
            dummyPolls.Add(timedPoll);

            _bobVote = new Vote() { Id = 1, Ballot = _bobBallot, Choice = burgerChoice, Poll = _mainPoll };
            _dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, Poll = _mainPoll, Choice = new Choice() { Id = 1 }, Ballot = _joeBallot };
            _dummyVotes.Add(_joeVote);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Choices).Returns(dummyChoices);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            var mockValidator = new Mock<IVoteValidator>();

            _mockValidatorFactory = new Mock<IVoteValidatorFactory>();
            _mockValidatorFactory.Setup(a => a.CreateValidator(PollType.Basic)).Returns(mockValidator.Object);

            _mockMetricHandler = new Mock<IMetricHandler>();

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
            Assert.AreEqual(1, response.Single().ChoiceId);
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
            _controller.ModelState.AddModelError("ChoiceId", "");

            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new BallotRequestModel());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutNonexistentChoiceIsNotAllowed()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 99 } } });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PutNonexistentPollIsNotAllowed()
        {
            // Act
            _controller.Put(Guid.NewGuid(), _bobBallot.TokenGuid, new BallotRequestModel() { });
        }

        [TestMethod]
        public void PutWithNewVoteIsAllowed()
        {
            // Act
            _controller.Put(_mainUUID, _otherBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1, VoteValue = 0 } } });

            // Assert
            Assert.AreEqual(3, _dummyVotes.Local.Count);
        }

        [TestMethod]
        public void PutReplacesCurrentVote()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1, VoteValue = 0 } } });

            // Assert
            Assert.AreEqual(2, _dummyVotes.Local.Count);
        }


        [TestMethod]
        public void PutWithNewVoteSetsVoteValueCorrectly()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1, VoteValue = 0 } } });

            // Assert
            Assert.AreEqual(0, _dummyVotes.Local.Single(v => v.Ballot.TokenGuid == _bobBallot.TokenGuid && v.Poll.UUID == _mainUUID).VoteValue);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutInvalidChoiceIsNotAllowed()
        {
            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 3 } } });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
        public void PutOnAnExpiredPollNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { Choice = new Choice() { Id = 1 }, Poll = new Poll() { UUID = _timedUUID } };

            // Act
            _controller.Put(_timedUUID, _bobBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1 } } });
        }

        [TestMethod]
        public void PutSelectsCorrectValidatorForTypeOfPoll()
        {
            // Arrange
            var pointsValidator = new Mock<IVoteValidator>();
            _mockValidatorFactory.Setup(a => a.CreateValidator(PollType.Points)).Returns(pointsValidator.Object);

            // Act
            _controller.Put(_pointsUUID, _otherBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel { ChoiceId = 1, VoteValue = 2 } } });

            // Assert
            pointsValidator.Verify(a => a.Validate(It.IsAny<List<VoteRequestModel>>(), It.IsAny<Poll>(), It.IsAny<ModelStateDictionary>()));
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
        public void PutOnInviteOnlyPollWithUnrecognisedTokenIsForbidden()
        {
            // Arrange
            var ballotRequest = new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1 } } };

            // Act
            _controller.Put(_tokenUUID, Guid.NewGuid(), ballotRequest);
        }

        [TestMethod]
        public void PutOnOpenPollWithUnrecognisedTokenIsAllowed()
        {
            // Arrange
            var ballotRequest = new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1 } } };

            // Act
            _controller.Put(_mainUUID, Guid.NewGuid(), ballotRequest);
        }

        [TestMethod]
        public void PutNewVoteGeneratesAddVoteMetric()
        {
            // Arrange
            var ballotRequest = new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1 } } };

            // Act
            _controller.Put(_mainUUID, Guid.NewGuid(), ballotRequest);

            // Assert
            _mockMetricHandler.Verify(m => m.HandleVoteAddedEvent(It.Is<Vote>(v => v.Choice.Id == 1), _mainUUID), Times.Once());
        }

        [TestMethod]
        public void ClearingVoteGeneratesRemoveVoteMetric()
        {
            // Arrange
            Guid tokenGuid = Guid.NewGuid();
            var existingVote = new Vote() { Choice = new Choice() { Id = 1 }, Poll = new Poll() { UUID = _mainUUID }, Ballot = new Ballot() { TokenGuid = tokenGuid } };
            _dummyVotes.Add(existingVote);
            var ballotRequest = new BallotRequestModel();

            // Act
            _controller.Put(_mainUUID, tokenGuid, ballotRequest);

            // Assert
            _mockMetricHandler.Verify(m => m.HandleVoteDeletedEvent(existingVote, _mainUUID), Times.Once());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutWithIsElectionModeNotAllowed()
        {
            // Arrange
            _mainPoll.IsElectionMode = true;

            // Act
            _controller.Put(_mainUUID, _bobBallot.TokenGuid, new BallotRequestModel() { Votes = new List<VoteRequestModel>() { new VoteRequestModel() { ChoiceId = 1, VoteValue = 0 } } });
        }

        #endregion
    }
}
