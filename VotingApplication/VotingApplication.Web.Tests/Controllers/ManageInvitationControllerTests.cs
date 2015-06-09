using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Tests.Controllers
{
    [TestClass]
    public class ManageInvitationControllerTests
    {
        private ManageInvitationController _controller;
        private Guid _mainManageID;
        private Poll _mainPoll;
        private Guid _inviteOnlyManageID;
        private Guid _inviteOnlyPollID;
        private Poll _inviteOnlyPoll;
        private Mock<ICorrespondenceService> _mockInvitationService;
        private Mock<IMetricHandler> _mockMetricHandler;
        private InMemoryDbSet<Vote> _dummyVotes;
        private InMemoryDbSet<Ballot> _dummyBallots;

        [TestInitialize]
        public void setup()
        {
            _mainManageID = Guid.NewGuid();
            _inviteOnlyManageID = Guid.NewGuid();
            _inviteOnlyPollID = Guid.NewGuid();
            _mainPoll = new Poll() { ManageId = _mainManageID, Name = "Main Poll", UUID = Guid.NewGuid(), Ballots = new List<Ballot>() };
            _inviteOnlyPoll = new Poll() { ManageId = _inviteOnlyManageID, UUID = _inviteOnlyPollID, InviteOnly = true, Ballots = new List<Ballot>() };

            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);
            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(_inviteOnlyPoll);

            _dummyVotes = new InMemoryDbSet<Vote>(true);
            _dummyBallots = new InMemoryDbSet<Ballot>(true);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Ballots).Returns(_dummyBallots);

            _mockInvitationService = new Mock<ICorrespondenceService>();
            _mockMetricHandler = new Mock<IMetricHandler>();

            _controller = new ManageInvitationController(mockContextFactory.Object, _mockMetricHandler.Object, _mockInvitationService.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            _controller.Get(_mainManageID);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetRejectsUnrecognisedPollId()
        {
            // Act
            _controller.Get(Guid.NewGuid());
        }

        [TestMethod]
        public void GetReturnsAllBallotsForMatchingPoll()
        {
            Guid ballotToken = Guid.NewGuid();
            Ballot mainPollBallot = new Ballot() { ManageGuid = ballotToken, Email = "a@b.c" };
            _mainPoll.Ballots.Add(mainPollBallot);

            // Act
            List<ManageInvitationResponseModel> responseBallots = _controller.Get(_mainManageID);
            List<Guid> expectedManageTokens = new List<Guid> { ballotToken };
            List<Guid> actualManageTokens = responseBallots.Select(b => b.ManageToken).ToList<Guid>();
            CollectionAssert.AreEquivalent(expectedManageTokens, actualManageTokens);
        }

        [TestMethod]
        public void GetIgnoresBallotsFromOtherPolls()
        {
            Guid ballotToken = Guid.NewGuid();
            Ballot mainPollBallot = new Ballot() { ManageGuid = ballotToken, Email = "a@b.c" };
            _mainPoll.Ballots.Add(mainPollBallot);

            // Act
            List<ManageInvitationResponseModel> responseBallots = _controller.Get(_inviteOnlyManageID);
            List<Guid> expectedManageTokens = new List<Guid>();
            List<Guid> actualManageTokens = responseBallots.Select(b => b.ManageToken).ToList<Guid>();
            CollectionAssert.AreEquivalent(expectedManageTokens, actualManageTokens);
        }

        [TestMethod]
        public void GetIgnoresBallotsWithoutEmails()
        {
            Guid ballotToken = Guid.NewGuid();
            Ballot mainPollBallot = new Ballot() { ManageGuid = ballotToken, Email = null };
            _mainPoll.Ballots.Add(mainPollBallot);

            // Act
            List<ManageInvitationResponseModel> responseBallots = _controller.Get(_mainManageID);
            List<Guid> expectedManageTokens = new List<Guid>();
            List<Guid> actualManageTokens = responseBallots.Select(b => b.ManageToken).ToList<Guid>();
            CollectionAssert.AreEquivalent(expectedManageTokens, actualManageTokens);
        }

        [TestMethod]
        public void GetIgnoresBallotsWithEmptyEmails()
        {
            Guid ballotToken = Guid.NewGuid();
            Ballot mainPollBallot = new Ballot() { ManageGuid = ballotToken, Email = null };
            _mainPoll.Ballots.Add(mainPollBallot);

            // Act
            List<ManageInvitationResponseModel> responseBallots = _controller.Get(_mainManageID);
            List<Guid> expectedManageTokens = new List<Guid>();
            List<Guid> actualManageTokens = responseBallots.Select(b => b.ManageToken).ToList<Guid>();
            CollectionAssert.AreEquivalent(expectedManageTokens, actualManageTokens);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act		
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PostWithInvalidPollIdIsRejected()
        {
            // Act		
            _controller.Post(Guid.NewGuid(), new List<ManageInvitationRequestModel>());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostWithNullInviteesIsRejected()
        {
            // Act		
            _controller.Post(_mainManageID, null);
        }


        [TestMethod]
        public void PostCanHandleBallotsWithoutEmails()
        {
            // Arrange		
            Ballot nullEmailBallot = new Ballot() { Email = null, TokenGuid = Guid.Empty };
            _inviteOnlyPoll.Ballots = new List<Ballot> { nullEmailBallot };
            var request = new ManageInvitationRequestModel { Email = "a@b.c" };

            // Act		
            _controller.Post(_inviteOnlyManageID, new List<ManageInvitationRequestModel> { request });

            // Assert - No error thrown
        }

        [TestMethod]
        public void PostOfPendingNewEmailIsSavedAsPendingBallot()
        {
            // Arrange
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = false };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            Ballot newBallot = _mainPoll.Ballots.FirstOrDefault();
            Assert.IsNotNull(newBallot);
            Assert.AreNotEqual(Guid.Empty, newBallot.ManageGuid);
            Assert.AreEqual(Guid.Empty, newBallot.TokenGuid);
            Assert.AreEqual("a@b.c", newBallot.Email);
        }

        [TestMethod]
        public void PostOfNewEmailIsSavedAsInvitedBallot()
        {
            // Arrange
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            Ballot newBallot = _mainPoll.Ballots.FirstOrDefault();
            Assert.IsNotNull(newBallot);
            Assert.AreNotEqual(Guid.Empty, newBallot.ManageGuid);
            Assert.AreNotEqual(Guid.Empty, newBallot.TokenGuid);
            Assert.AreEqual("a@b.c", newBallot.Email);
        }

        [TestMethod]
        public void PostOfNewEmailIsSentAnInvitation()
        {
            // Arrange
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            Ballot newBallot = _mainPoll.Ballots.FirstOrDefault();
            _mockInvitationService.Verify(mis => mis.SendInvitation(_mainPoll.UUID, newBallot, _mainPoll.Name));
        }

        [TestMethod]
        public void PostOfExistingPendingEmailIsLeftAsPending()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Ballot pendingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken };
            _dummyBallots.Add(pendingBallot);
            _mainPoll.Ballots.Add(pendingBallot);
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = false, ManageToken = manageToken };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            Ballot newBallot = _mainPoll.Ballots.FirstOrDefault();
            Assert.IsNotNull(newBallot);
            Assert.AreEqual(manageToken, newBallot.ManageGuid);
            Assert.AreEqual(Guid.Empty, newBallot.TokenGuid);
            Assert.AreEqual("a@b.c", newBallot.Email);
        }

        [TestMethod]
        public void PostOfExistingEmailIsSavedAsInvitedBallot()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Ballot pendingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken };
            _dummyBallots.Add(pendingBallot);
            _mainPoll.Ballots.Add(pendingBallot);
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true, ManageToken = manageToken };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            Ballot newBallot = _mainPoll.Ballots.FirstOrDefault();
            Assert.IsNotNull(newBallot);
            Assert.AreEqual(manageToken, newBallot.ManageGuid);
            Assert.AreNotEqual(Guid.Empty, newBallot.TokenGuid);
            Assert.AreEqual("a@b.c", newBallot.Email);
        }

        [TestMethod]
        public void PostOfExistingEmailIsSentAnInvitation()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Ballot pendingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken };
            _dummyBallots.Add(pendingBallot);
            _mainPoll.Ballots.Add(pendingBallot);
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true, ManageToken = manageToken };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            _mockInvitationService.Verify(mis => mis.SendInvitation(_mainPoll.UUID, pendingBallot, _mainPoll.Name));
        }

        [TestMethod]
        public void PostWithoutExistingPendingBallotDeletesTheExistingBallot()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Ballot pendingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken };
            _dummyBallots.Add(pendingBallot);
            _mainPoll.Ballots.Add(pendingBallot);

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());

            // Assert
            CollectionAssert.AreEquivalent(new List<Ballot>(), _mainPoll.Ballots);
        }

        [TestMethod]
        public void PostWithoutExistingPendingBallotDeletesTheVotesOfThatBallot()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Vote voteToRemove = new Vote();
            Ballot pendingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken, Votes = new List<Vote> { voteToRemove }};
            _dummyBallots.Add(pendingBallot);
            _mainPoll.Ballots.Add(pendingBallot);
            _dummyVotes.Add(voteToRemove);

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>(), _dummyVotes.Local);
        }

        [TestMethod]
        public void PostOfExistingInvitedEmailIsLeftAsInvited()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Guid ballotToken = Guid.NewGuid();
            Ballot existingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken, TokenGuid = ballotToken };
            _dummyBallots.Add(existingBallot);
            _mainPoll.Ballots.Add(existingBallot);
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = false, ManageToken = manageToken };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            Ballot newBallot = _mainPoll.Ballots.FirstOrDefault();
            Assert.IsNotNull(newBallot);
            Assert.AreEqual(manageToken, newBallot.ManageGuid);
            Assert.AreEqual(ballotToken, newBallot.TokenGuid);
            Assert.AreEqual("a@b.c", newBallot.Email);
        }

        [TestMethod]
        public void PostOfExistingInvitedEmailIsUntouchedByNewInvitation()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Guid ballotToken = Guid.NewGuid();
            Ballot existingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken, TokenGuid = ballotToken };
            _dummyBallots.Add(existingBallot);
            _mainPoll.Ballots.Add(existingBallot);
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true, ManageToken = manageToken };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            Ballot newBallot = _mainPoll.Ballots.FirstOrDefault();
            Assert.IsNotNull(newBallot);
            Assert.AreEqual(manageToken, newBallot.ManageGuid);
            Assert.AreEqual(ballotToken, newBallot.TokenGuid);
            Assert.AreEqual("a@b.c", newBallot.Email);
        }

        [TestMethod]
        public void PostOfExistingInvitedEmailIsReissuedAnInvitation()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Guid ballotToken = Guid.NewGuid();
            Ballot existingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken, TokenGuid = ballotToken };
            _dummyBallots.Add(existingBallot);
            _mainPoll.Ballots.Add(existingBallot);
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true, ManageToken = manageToken };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            _mockInvitationService.Verify(mis => mis.SendInvitation(_mainPoll.UUID, existingBallot, _mainPoll.Name));
        }

        [TestMethod]
        public void PostWithoutExistingInvitedBallotDeletesTheExistingBallot()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Guid ballotToken = Guid.NewGuid();
            Ballot existingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken, TokenGuid = ballotToken };
            _dummyBallots.Add(existingBallot);
            _mainPoll.Ballots.Add(existingBallot);

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());

            // Assert
            CollectionAssert.AreEquivalent(new List<Ballot>(), _mainPoll.Ballots);
        }

        [TestMethod]
        public void PostWithoutExistingInvitedBallotDeletesTheVotesOfThatBallot()
        {
            // Arrange
            Guid manageToken = Guid.NewGuid();
            Guid ballotToken = Guid.NewGuid();
            Vote voteToRemove = new Vote();
            Ballot existingBallot = new Ballot() { Email = "a@b.c", ManageGuid = manageToken, TokenGuid = ballotToken, Votes = new List<Vote> { voteToRemove } };
            _dummyBallots.Add(existingBallot);
            _mainPoll.Ballots.Add(existingBallot);
            _dummyVotes.Add(voteToRemove);

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>(), _dummyVotes.Local);
        }
        
        [TestMethod]
        public void DeletingTheVotesOfABallotGeneratesAVoteDeletionMetric()
        {
            // Arrange
            Vote voteToRemove = new Vote();
            Ballot existingBallot = new Ballot() { Votes = new List<Vote> { voteToRemove } };
            _dummyBallots.Add(existingBallot);
            _mainPoll.Ballots.Add(existingBallot);
            _dummyVotes.Add(voteToRemove);

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());

            // Assert
            _mockMetricHandler.Verify(m => m.HandleVoteDeletedEvent(voteToRemove, _mainPoll.UUID), Times.Once());
        }

        [TestMethod]
        public void AddingANewInvitationGeneratesAnAddBallotMetric()
        {
            // Arrange
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            _mockMetricHandler.Verify(m => m.HandleBallotAddedEvent(It.Is<Ballot>(b => b.Email == "a@b.c"), _mainPoll.UUID), Times.Once());
        }

        [TestMethod]
        public void SavingAnEmailWithoutSendingInvitationDoesNotGenerateANewBallotMetric()
        {
            // Arrange
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = false };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            _mockMetricHandler.Verify(m => m.HandleBallotAddedEvent(It.IsAny<Ballot>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        public void ResendingEmailToAnExistingBallotDoesNotGenerateNewBallotMetric()
        {
            // Arrange
            Ballot existingBallot = new Ballot { Email = "a@b.c", TokenGuid = Guid.NewGuid(), ManageGuid = Guid.NewGuid() };
            _mainPoll.Ballots.Add(existingBallot);
            _dummyBallots.Add(existingBallot);
            var request = new ManageInvitationRequestModel { Email = "a@b.c", SendInvitation = true, ManageToken = existingBallot.ManageGuid  };

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel> { request });

            // Assert
            _mockMetricHandler.Verify(m => m.HandleBallotAddedEvent(It.IsAny<Ballot>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        public void DeletingAPendingBallotDoesNotGenerateABallotDeletedMetric()
        {
            // Arrange
            Ballot pendingBallot = new Ballot { Email = "a@b.c", ManageGuid = Guid.NewGuid() };
            _mainPoll.Ballots.Add(pendingBallot);
            _dummyBallots.Add(pendingBallot);

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());

            // Assert
            _mockMetricHandler.Verify(m => m.HandleBallotDeletedEvent(It.IsAny<Ballot>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        public void DeletingAnInvitedBallotGeneratesABallotDeletedMetric()
        {

            // Arrange
            Ballot invitedBallot = new Ballot { Email = "a@b.c", ManageGuid = Guid.NewGuid(), TokenGuid = Guid.NewGuid() };
            _mainPoll.Ballots.Add(invitedBallot);
            _dummyBallots.Add(invitedBallot);

            // Act
            _controller.Post(_mainManageID, new List<ManageInvitationRequestModel>());

            // Assert
            _mockMetricHandler.Verify(m => m.HandleBallotDeletedEvent(invitedBallot, _mainPoll.UUID), Times.Once());
        }

        #endregion
    }
}
