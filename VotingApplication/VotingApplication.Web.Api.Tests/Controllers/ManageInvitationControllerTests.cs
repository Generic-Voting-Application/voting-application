using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class ManageInvitationControllerTests
    {
        private ManageInvitationController _controller;
        private Guid _mainUUID;
        private Poll _mainPoll;
        private Guid _inviteOnlyUUID;
        private Poll _inviteOnlyPoll;
        private Mock<IMailSender> _mockMailSender;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _inviteOnlyUUID = Guid.NewGuid();
            _mainPoll = new Poll() { ManageId = _mainUUID, Ballots = new List<Ballot>() };
            _inviteOnlyPoll = new Poll() { ManageId = _inviteOnlyUUID, InviteOnly = true, Ballots = new List<Ballot>() };

            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);
            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(_inviteOnlyPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);

            _mockMailSender = new Mock<IMailSender>();

            _controller = new ManageInvitationController(mockContextFactory.Object, _mockMailSender.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            _controller.Post(_mainUUID);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PostWithInvalidPollIdIsRejected()
        {
            // Act
            _controller.Post(Guid.NewGuid());
        }

        [TestMethod]
        public void PostDoesNotCreatesNewTokensForOpenPoll()
        {
            // Act
            _controller.Post(_mainUUID);

            // Assert
            Assert.AreEqual(0, _mainPoll.Ballots.Count);
        }

        [TestMethod]
        public void PostCreatesNewTokensForInviteOnlyPoll()
        {
            // Arrange
            var pendingInvitation = new Ballot() { Email = "a@b.c", TokenGuid = Guid.Empty };
            _inviteOnlyPoll.Ballots = new List<Ballot> { pendingInvitation };

            // Act
            _controller.Post(_inviteOnlyUUID);

            // Assert
            Assert.AreNotEqual(Guid.Empty, pendingInvitation.TokenGuid);
        }

        [TestMethod]
        public void PostSendsEmailForPendingInvitation()
        {
            // Arrange
            var pendingInvitation = new Ballot() { Email = "a@b.c", TokenGuid = Guid.Empty };
            _inviteOnlyPoll.Ballots = new List<Ballot> { pendingInvitation };

            // Act
            _controller.Post(_inviteOnlyUUID);

            // Assert
            _mockMailSender.Verify(ms => ms.SendMail("a@b.c", It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        public void PostDoesNotCreatesNewTokensForInvitationsWithExistingTokens()
        {
            // Arrange
            Guid existingToken = Guid.NewGuid();
            var sentInvitation = new Ballot() { Email = "d@e.f", TokenGuid = existingToken };
            _inviteOnlyPoll.Ballots = new List<Ballot> { sentInvitation };

            // Act
            _controller.Post(_inviteOnlyUUID);

            // Assert
            Assert.AreEqual(existingToken, sentInvitation.TokenGuid);
        }

        #endregion
    }
}
