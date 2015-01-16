using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;

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
        private Guid[] UUIDs;
        private Option _redOption;
        private InMemoryDbSet<Poll> _dummyPolls;

        [TestInitialize]
        public void setup()
        {
            _redOption = new Option() { Name = "Red" };

            UUIDs = new [] {Guid.NewGuid(), Guid.NewGuid(), _templateUUID, Guid.NewGuid()};
            _mainPoll = new Poll() { UUID = UUIDs[0], ManageId = Guid.NewGuid() };
            _otherPoll = new Poll() { UUID = UUIDs[1], ManageId = Guid.NewGuid() };
            _templateUUID = Guid.NewGuid();
            _templatePoll = new Poll() { UUID = _templateUUID, ManageId = Guid.NewGuid(), Options = new List<Option>() { _redOption }, CreatorIdentity = "a@b.c" };

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

            _controller = new PollController(mockContextFactory.Object, mockMailSender.Object);
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
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
            List<Poll> expectedPolls = new List<Poll>() { _templatePoll };
            List<Poll> responsePolls = ((ObjectContent)response.Content).Value as List<Poll>;
            CollectionAssert.AreEquivalent(expectedPolls, responsePolls);
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
            List<Poll> responsePolls = ((ObjectContent)response.Content).Value as List<Poll>;
            CollectionAssert.AreEquivalent(new List<Poll>(), responsePolls);
        }

        [TestMethod]
        public void GetByIdIsAllowed()
        {
            // Act
            var response = _controller.Get(UUIDs[0]);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetByIdOnNonexistentPollsAreNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new Poll());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Poll());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(new PollCreationRequestModel() { Name = "New Poll" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostRejectsPollWithInvalidInput()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "");

            // Act
            var response = _controller.Post(new PollCreationRequestModel());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PostAssignsPollUUID()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);
            PollCreationResponseModel responseModel = ((ObjectContent)response.Content).Value as PollCreationResponseModel;
           
            // Assert
            Assert.AreNotEqual(Guid.Empty, responseModel.UUID);
        }

        [TestMethod]
        public void PostAssignsPollManageId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);
            PollCreationResponseModel responseModel = ((ObjectContent)response.Content).Value as PollCreationResponseModel;

            // Assert
            Assert.AreNotEqual(Guid.Empty, responseModel.ManageId);
        }
        
        [TestMethod]
        public void PostAssignsPollManageIdDifferentFromPollId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);
            PollCreationResponseModel responseModel = ((ObjectContent)response.Content).Value as PollCreationResponseModel;

            // Assert
            Assert.AreNotEqual(responseModel.UUID, responseModel.ManageId);
        }

        [TestMethod]
        public void PostWithAuthorizationSetsUsernameOfPollOwner()
        {
            // Arrange
            var identity = new System.Security.Principal.GenericIdentity("newUser@b.c");
            var user = new System.Security.Principal.GenericPrincipal(identity, new string[0]);
            _controller.User = user;

            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            _controller.Post(newPoll);

            // Assert
            Poll createdPoll = _dummyPolls.Last();
            Assert.AreEqual("newUser@b.c", createdPoll.CreatorIdentity);
        }

        [TestMethod]
        public void PostReturnsIDsOfNewPoll()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);
            PollCreationResponseModel responseModel = ((ObjectContent)response.Content).Value as PollCreationResponseModel;

            // Assert
            Assert.AreEqual(_dummyPolls.Last().UUID, responseModel.UUID);
            Assert.AreEqual(_dummyPolls.Last().ManageId, responseModel.ManageId);
        }

        [TestMethod]
        public void PostAddsNewPollToPolls()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { Name = "New Poll" };
            var response = _controller.Post(newPoll);
            PollCreationResponseModel responseModel = ((ObjectContent)response.Content).Value as PollCreationResponseModel;

            // Assert
            Assert.AreEqual(_dummyPolls.Count(), 4);
        }  
        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }
        #endregion
        
    }
}
