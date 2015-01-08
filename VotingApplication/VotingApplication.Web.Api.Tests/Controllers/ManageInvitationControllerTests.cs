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
            _mainPoll = new Poll() { ManageID = _mainUUID, Tokens = new List<Token>() };
            _inviteOnlyPoll = new Poll() { ManageID = _inviteOnlyUUID, InviteOnly = true, Tokens = new List<Token>() };
            
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

        #region GET

        [TestMethod]
        public void GetIsNotAllowed()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
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

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, new List<string>());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostWithInvalidPollIdIsRejected()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Post(newGuid, new List<string>());

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void PostCreatesNewTokensForInviteOnlyPoll()
        {
            // Act
            var invitations = new List<string>() { "a@b.c", "d@e.f" };
            _controller.Post(_inviteOnlyUUID, invitations);

            // Assert
            Assert.AreEqual(2, _inviteOnlyPoll.Tokens.Count);
        }

        [TestMethod]
        public void PostDoesNotCreatesNewTokensForOpenPoll()
        {
            // Act
            _controller.Post(_mainUUID, new List<string>());

            // Assert
            Assert.AreEqual(0, _mainPoll.Tokens.Count);
        }

        [TestMethod]
        public void PostDoesNotCreatesNewTokensForMalformedEmailsInInviteOnlyPoll()
        {
            // Act
            var invitations = new List<string>() { "a@b.c", "d@e.f", @"notAnEmail", "also@notAnEmail", @"neither@is.this.", @"    but@this.is  " };
            _controller.Post(_inviteOnlyUUID, invitations);

            // Assert
            Assert.AreEqual(3, _inviteOnlyPoll.Tokens.Count);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, 1, null);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_mainUUID, new List<string>());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_mainUUID, 1, null);

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
