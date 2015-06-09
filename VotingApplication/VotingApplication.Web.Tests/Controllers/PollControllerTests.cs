﻿using FakeDbSet;
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
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Tests.Controllers
{
    [TestClass]
    public class PollControllerTests
    {
        const string UserId = "4AEAE121-D540-48BF-907A-AA454248C0C0";

        private PollController _controller;
        private Mock<IMetricHandler> _metricHandler;
        private Poll _mainPoll;
        private Poll _otherPoll;
        private Poll _templatePoll;
        private Guid _templateUUID;
        private DateTime _templateCreatedDate;
        private Guid[] UUIDs;
        private Choice _redChoice;
        private InMemoryDbSet<Poll> _dummyPolls;

        [TestInitialize]
        public void setup()
        {
            _redChoice = new Choice() { Name = "Red" };

            UUIDs = new[] { Guid.NewGuid(), Guid.NewGuid(), _templateUUID, Guid.NewGuid() };
            _mainPoll = new Poll() { UUID = UUIDs[0], ManageId = Guid.NewGuid() };
            _otherPoll = new Poll() { UUID = UUIDs[1], ManageId = Guid.NewGuid() };

            _templateUUID = Guid.NewGuid();
            _templateCreatedDate = DateTime.UtcNow.AddDays(-5);
            _templatePoll = new Poll()
            {
                UUID = _templateUUID,
                ManageId = Guid.NewGuid(),
                CreatedDateUtc = _templateCreatedDate,
                Choices = new List<Choice>() { _redChoice },
                CreatorIdentity = UserId
            };

            _dummyPolls = new InMemoryDbSet<Poll>(true) { _mainPoll, _otherPoll, _templatePoll };

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _metricHandler = new Mock<IMetricHandler>();

            _controller = new PollController(mockContextFactory.Object, _metricHandler.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
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
            _controller.Post(new PollCreationRequestModel() { PollName = "New Poll" });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostRejectsPollWithInvalidInput()
        {
            // Arrange
            _controller.ModelState.AddModelError("PollName", "");

            // Act
            _controller.Post(new PollCreationRequestModel());
        }

        [TestMethod]
        public void PostAssignsPollUUID()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, response.UUID);
        }

        [TestMethod]
        public void PostAssignsPollManageId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, response.ManageId);
        }

        [TestMethod]
        public void PostAssignsPollManageIdDifferentFromPollId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(response.UUID, response.ManageId);
        }

        [TestMethod]
        public void PostWithAuthorizationSetsUsernameOfPollOwner()
        {
            // Mocking of GetUserId() taken from http://stackoverflow.com/questions/22762338/how-do-i-mock-user-identity-getuserid

            var claim = new Claim("test", UserId);
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
                PollName = "New Poll"
            };


            _controller.Post(newPoll);


            Poll createdPoll = _dummyPolls.Last();

            Assert.AreEqual(UserId, createdPoll.CreatorIdentity);
        }

        [TestMethod]
        public void PostReturnsIDsOfNewPoll()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreEqual(_dummyPolls.Last().UUID, response.UUID);
            Assert.AreEqual(_dummyPolls.Last().ManageId, response.ManageId);
        }

        [TestMethod]
        public void PostAddsNewPollToPolls()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreEqual(_dummyPolls.Count(), 4);
        }

        [TestMethod]
        public void SuccessfulPollCreationGeneratesMetric()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            _metricHandler.Verify(m => m.HandlePollCreatedEvent(It.Is<Poll>(p => p.Name == "New Poll" && p.UUID == response.UUID)), Times.Once());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void UnsuccessfulPollCreationDoesNotGenerateMetric()
        {
            // Act
            var response = _controller.Post(null);

            // Assert
            _metricHandler.Verify(m => m.HandlePollCreatedEvent(It.IsAny<Poll>()), Times.Never());
        }

        #endregion

    }
}