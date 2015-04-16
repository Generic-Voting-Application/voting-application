using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class PollTokenControllerTests
    {
        private PollTokenController _controller;
        private Guid _mainUUID;
        private Guid _inviteUUID;
        private Poll _mainPoll;
        private Poll _invitePoll;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _inviteUUID = Guid.NewGuid();

            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);
            _mainPoll = new Poll() { UUID = _mainUUID };
            _invitePoll = new Poll() { UUID = _inviteUUID };
            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(_invitePoll);

            _invitePoll.InviteOnly = true;

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Returns(null);

            _controller = new PollTokenController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetReturnsNewGuid()
        {
            // Act
            var response = _controller.Get(_mainUUID);
            Guid responseGuid;

            bool isGuid = Guid.TryParse(response.ToString(), out responseGuid);

            // Assert
            Assert.IsTrue(isGuid);
        }

        [TestMethod]
        public void GetSavesNewToken()
        {
            // Act
            _controller.Get(_mainUUID);

            // Assert
            Assert.AreEqual(1, _mainPoll.Ballots.Count);
        }

        [TestMethod]
        public void GetSavesNewManageToken()
        {
            // Act
            _controller.Get(_mainUUID);

            Guid manageGuid;
            bool isGuid = Guid.TryParse(_mainPoll.Ballots.Last().ManageGuid.ToString(), out manageGuid);

            // Assert
            Assert.IsTrue(isGuid);
            Assert.AreNotEqual(manageGuid, Guid.Empty);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetFailsOnNonExistantPoll()
        {
            // Act
            _controller.Get(Guid.NewGuid());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
        public void GetFailsOnInviteOnlyPoll()
        {
            // Act
            _controller.Get(_inviteUUID);

        }

        #endregion
    }
}
