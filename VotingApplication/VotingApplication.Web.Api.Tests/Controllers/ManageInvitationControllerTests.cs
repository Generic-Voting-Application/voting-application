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
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Api.Tests.Controllers
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
        private Mock<IInvitationService> _mockInvitationService;

        [TestInitialize]
        public void setup()
        {
            _mainManageID = Guid.NewGuid();
            _inviteOnlyManageID = Guid.NewGuid();
            _inviteOnlyPollID = Guid.NewGuid();
            _mainPoll = new Poll() { ManageId = _mainManageID, Ballots = new List<Ballot>() };
            _inviteOnlyPoll = new Poll() { ManageId = _inviteOnlyManageID, UUID = _inviteOnlyPollID, InviteOnly = true, Ballots = new List<Ballot>() };

            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);
            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(_inviteOnlyPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);

            _mockInvitationService = new Mock<IInvitationService>();

            _controller = new ManageInvitationController(mockContextFactory.Object, _mockInvitationService.Object);
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
            Ballot mainPollBallot = new Ballot() { ManageGuid = ballotToken };
            _mainPoll.Ballots.Add(mainPollBallot);

            // Act
            List<ManagePollBallotRequestModel> responseBallots = _controller.Get(_mainManageID);
            List<Guid> expectedManageTokens = new List<Guid> { ballotToken };
            List<Guid> actualManageTokens = responseBallots.Select(b => b.ManageToken).ToList<Guid>();
            CollectionAssert.AreEquivalent(expectedManageTokens, actualManageTokens);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act		
            _controller.Post(_mainManageID, new List<ManagePollBallotRequestModel>());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PostWithInvalidPollIdIsRejected()
        {
            // Act		
            _controller.Post(Guid.NewGuid(), new List<ManagePollBallotRequestModel>());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostWithNullInviteesIsRejected()
        {
            // Act		
            _controller.Post(_mainManageID, null);
        }


        [TestMethod]
        public void PostDoesNotCreatesNewTokensForOpenPoll()
        {
            // Act		
            _controller.Post(_mainManageID, new List<ManagePollBallotRequestModel>());

            // Assert		
            Assert.AreEqual(0, _mainPoll.Ballots.Count);
        }

        [TestMethod]
        public void PostCanHandleBallotsWithoutEmails()
        {
            // Arrange		
            Ballot nullEmailBallot = new Ballot() { Email = null, TokenGuid = Guid.Empty };
            _inviteOnlyPoll.Ballots = new List<Ballot> { nullEmailBallot };
            var request = new ManagePollBallotRequestModel { Email = "a@b.c" };

            // Act		
            _controller.Post(_inviteOnlyManageID, new List<ManagePollBallotRequestModel> { request });

            // Assert - No error thrown
        }

        [TestMethod]
        public void PostCreatesNewTokensForInviteOnlyPoll()
        {
            // Arrange		
            Ballot pendingInvitation = new Ballot() { Email = "a@b.c", TokenGuid = Guid.Empty };
            _inviteOnlyPoll.Ballots = new List<Ballot> { pendingInvitation };
            var request = new ManagePollBallotRequestModel { Email = pendingInvitation.Email };

            // Act		
            _controller.Post(_inviteOnlyManageID, new List<ManagePollBallotRequestModel> { request });

            // Assert		
            Assert.AreNotEqual(Guid.Empty, pendingInvitation.TokenGuid);
        }

        [TestMethod]
        public void PostCreatesNewBallotIfNoMatchingEmailExists()
        {
            // Arrange		
            _inviteOnlyPoll.Ballots = new List<Ballot> { };
            var request = new ManagePollBallotRequestModel { Email = "a@b.c" };

            // Act		
            _controller.Post(_inviteOnlyManageID, new List<ManagePollBallotRequestModel> { request });

            // Assert		
            List<string> expectedEmails = new List<string> { "a@b.c" };
            List<string> actualEmails = _inviteOnlyPoll.Ballots.Select(s => s.Email).ToList<string>();
            CollectionAssert.AreEquivalent(expectedEmails, actualEmails);
        }

        [TestMethod]
        public void PostSendsEmailForPendingInvitation()
        {
            // Arrange		
            Ballot pendingInvitation = new Ballot() { Email = "a@b.c", TokenGuid = Guid.Empty };
            _inviteOnlyPoll.Ballots = new List<Ballot> { pendingInvitation };
            var request = new ManagePollBallotRequestModel { Email = pendingInvitation.Email };

            // Act		
            _controller.Post(_inviteOnlyManageID, new List<ManagePollBallotRequestModel> { request });

            // Assert		
            _mockInvitationService.Verify(mis => mis.SendInvitation(_inviteOnlyPollID, pendingInvitation, _inviteOnlyPoll.Name));
        }

        [TestMethod]
        public void PostDoesNotCreatesNewTokensForInvitationsWithExistingTokens()
        {
            // Arrange		
            Guid existingToken = Guid.NewGuid();
            Ballot sentInvitation = new Ballot() { Email = "d@e.f", TokenGuid = existingToken };
            _inviteOnlyPoll.Ballots = new List<Ballot> { sentInvitation };
            var request = new ManagePollBallotRequestModel { Email = sentInvitation.Email };

            // Act		
            _controller.Post(_inviteOnlyManageID, new List<ManagePollBallotRequestModel> { request });

            // Assert		
            Assert.AreEqual(existingToken, sentInvitation.TokenGuid);
        }

        #endregion
    }
}
