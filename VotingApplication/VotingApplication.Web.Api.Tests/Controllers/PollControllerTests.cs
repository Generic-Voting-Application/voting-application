using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class PollControllerTests
    {
        private PollController _controller;
        private Poll _mainPoll;
        private Poll _otherPoll;
        private Poll _templatePoll;
        private Guid _templateUUID;
        private DateTime _templateCreatedDate;
        private Guid[] UUIDs;
        private Option _redOption;
        private InMemoryDbSet<Poll> _dummyPolls;

        [TestInitialize]
        public void setup()
        {
            _redOption = new Option() { Name = "Red" };

            UUIDs = new[] { Guid.NewGuid(), Guid.NewGuid(), _templateUUID, Guid.NewGuid() };
            _mainPoll = new Poll() { UUID = UUIDs[0], ManageId = Guid.NewGuid() };
            _otherPoll = new Poll() { UUID = UUIDs[1], ManageId = Guid.NewGuid() };

            _templateUUID = Guid.NewGuid();
            _templateCreatedDate = DateTime.Now.AddDays(-5);
            _templatePoll = new Poll()
            {
                UUID = _templateUUID,
                ManageId = Guid.NewGuid(),
                CreatedDate = _templateCreatedDate,
                Options = new List<Option>() { _redOption },
                CreatorIdentity = "a@b.c"
            };

            _dummyPolls = new InMemoryDbSet<Poll>(true);
            _dummyPolls.Add(_mainPoll);
            _dummyPolls.Add(_otherPoll);
            _dummyPolls.Add(_templatePoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            var mockMailSender = new Mock<IMailSender>();

            _controller = new PollController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyPolls.Local.Count; i++)
            {
                _dummyPolls.Local[i].UUID = UUIDs[i];
            }
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            _controller.Get();
        }

        [TestMethod]
        public void GetWithAUserFetchesAllPollsByThatUser()
        {
            // Arrange
            var identity = new System.Security.Principal.GenericIdentity("a@b.c");
            var user = new System.Security.Principal.GenericPrincipal(identity, new string[0]);
            _controller.User = user;

            // Act
            var response = _controller.Get();

            // Assert
            var responsePoll = response.Single();
            Assert.AreEqual(_templatePoll.UUID, responsePoll.UUID);
            Assert.AreEqual(_templatePoll.Creator, responsePoll.Creator);
            Assert.AreEqual(_templatePoll.CreatedDate, responsePoll.CreatedDate);
        }

        [TestMethod]
        public void GetWithANewUserFetchesEmptyPollList()
        {
            // Arrange
            var identity = new System.Security.Principal.GenericIdentity("newUser@b.c");
            var user = new System.Security.Principal.GenericPrincipal(identity, new string[0]);
            _controller.User = user;

            // Act
            var response = _controller.Get();

            // Assert
            CollectionAssert.AreEquivalent(new List<Poll>(), response);
        }

        [TestMethod]
        public void GetByIdIsAllowed()
        {
            // Act
            _controller.Get(UUIDs[0]);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetByIdOnNonexistentPollsAreNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            _controller.Post(new PollCreationRequestModel() { Name = "New Poll" });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostRejectsPollWithInvalidInput()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "");

            // Act
            _controller.Post(new PollCreationRequestModel());
        }

        [TestMethod]
        public void PostAssignsPollUUID()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, response.UUID);
        }

        [TestMethod]
        public void PostAssignsPollManageId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, response.ManageId);
        }

        [TestMethod]
        public void PostAssignsPollManageIdDifferentFromPollId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(response.UUID, response.ManageId);
        }

        [TestMethod]
        public void PostWithAuthorizationSetsUsernameOfPollOwner()
        {
            // Mocking of GetUserId() taken from http://stackoverflow.com/questions/22762338/how-do-i-mock-user-identity-getuserid


            const string userId = "4AEAE121-D540-48BF-907A-AA454248C0C0";

            var claim = new Claim("test", userId);
            var mockIdentity = new Mock<ClaimsIdentity>();
            mockIdentity
                .Setup(ci => ci.FindFirst(It.IsAny<string>()))
                .Returns(claim);

            mockIdentity
                .Setup(i => i.IsAuthenticated)
                .Returns(true);

            var principal = new Mock<IPrincipal>();
            principal
                .Setup(ip => ip.Identity)
                .Returns(mockIdentity.Object);

            _controller.User = principal.Object;


            PollCreationRequestModel newPoll = new PollCreationRequestModel()
            {
                Name = "New Poll"
            };


            _controller.Post(newPoll);


            Poll createdPoll = _dummyPolls.Last();

            Assert.AreEqual(userId, createdPoll.CreatorIdentity);
        }

        [TestMethod]
        public void PostReturnsIDsOfNewPoll()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreEqual(_dummyPolls.Last().UUID, response.UUID);
            Assert.AreEqual(_dummyPolls.Last().ManageId, response.ManageId);
        }

        [TestMethod]
        public void PostAddsNewPollToPolls()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreEqual(_dummyPolls.Count(), 4);
        }
        #endregion

    }
}
