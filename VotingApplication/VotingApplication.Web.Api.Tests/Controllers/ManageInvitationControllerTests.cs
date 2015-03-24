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

            var mockMailSender = new Mock<IMailSender>();

            _controller = new ManageInvitationController(mockContextFactory.Object, mockMailSender.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            _controller.Post(_mainUUID, new List<string>());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PostWithInvalidPollIdIsRejected()
        {
            // Act
            _controller.Post(Guid.NewGuid(), new List<string>());
        }

        [TestMethod]
        public void PostDoesNotCreatesNewTokensForOpenPoll()
        {
            // Act
            _controller.Post(_mainUUID, new List<string>());

            // Assert
            Assert.AreEqual(0, _mainPoll.Ballots.Count);
        }

        [TestMethod]
        public void PostCreatesNewTokensForInviteOnlyPoll()
        {
            // Act
            var invitations = new List<string>() { "a@b.c", "d@e.f", @"    is@an.email  " };
            _controller.Post(_inviteOnlyUUID, invitations);

            // Assert
            Assert.AreEqual(3, _inviteOnlyPoll.Ballots.Count);
        }

        [TestMethod]
        public void PostDoesNotCreatesNewTokensForMalformedEmailsInInviteOnlyPoll()
        {
            // Act
            var invitations = new List<string>() { @"notAnEmail", "also@notAnEmail", @"neither@is.this." };
            _controller.Post(_inviteOnlyUUID, invitations);

            // Assert
            Assert.AreEqual(0, _inviteOnlyPoll.Ballots.Count);
        }

        #endregion
    }
}
