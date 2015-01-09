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

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class PollControllerTests
    {
        private PollController _controller;
        private Poll _mainPoll;
        private Poll _otherPoll;
        private Guid[] UUIDs;
        private Option _redOption;
        private InMemoryDbSet<Poll> _dummyPolls;

        [TestInitialize]
        public void setup()
        {
            _redOption = new Option() { Name = "Red" };
            Template emptyTemplate = new Template() { Id = 1 };
            Template redTemplate = new Template() { Id = 2, Options = new List<Option>() { _redOption } };
            InMemoryDbSet<Template> dummyTemplates = new InMemoryDbSet<Template>(true);
            dummyTemplates.Add(emptyTemplate);
            dummyTemplates.Add(redTemplate);

            UUIDs = new [] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
            _mainPoll = new Poll() { UUID = UUIDs[0], ManageID = Guid.NewGuid() };
            _otherPoll = new Poll() { UUID = UUIDs[1], ManageID = Guid.NewGuid() };

            _dummyPolls = new InMemoryDbSet<Poll>(true);
            _dummyPolls.Add(_mainPoll);
            _dummyPolls.Add(_otherPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.Templates).Returns(dummyTemplates);
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
        public void GetIsNotAllowed()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
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

        [TestMethod]
        public void GetByIdReturnsPollWithMatchingId()
        {
            // Act
            var response = _controller.Get(UUIDs[1]);

            // Assert
            Poll responsePoll = ((ObjectContent)response.Content).Value as Poll;
            Assert.AreEqual(_otherPoll, responsePoll);
        }

        [TestMethod]
        public void GetByIdDoesNotIncludeManageId()
        {
            // Act
            var response = _controller.Get(UUIDs[0]);

            // Assert
            Poll responsePoll = ((ObjectContent)response.Content).Value as Poll;
            Assert.AreEqual(Guid.Empty, responsePoll.ManageID);
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
            var response = _controller.Post(new Poll() { Name = "New Poll" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostRejectsPollWithMissingName()
        {
            // Act
            var response = _controller.Post(new Poll());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll did not have a name", error.Message);
        }

        [TestMethod]
        public void PostAcceptsPollWithMissingTemplate()
        {
            // Act
            var response = _controller.Post(new Poll() { Name = "New Poll" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostAddsEmptyOptionsListToPollWithoutTemplate()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            CollectionAssert.AreEquivalent(new List<Poll>(), newPoll.Options);
        }

        [TestMethod]
        public void PostPopulatesOptionsByTemplateId()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll", TemplateId = 2 };
            var response = _controller.Post(newPoll);

            // Assert
            List<Option> expectedOptions = new List<Option>() { _redOption };
            CollectionAssert.AreEquivalent(expectedOptions, newPoll.Options);
        }

        [TestMethod]
        public void PostRetainsSuppliedTemplate()
        {
            // Act
            List<Option> customOptions = new List<Option>() { _redOption };
            Poll newPoll = new Poll() { Name = "New Poll", Options = customOptions };
            var response = _controller.Post(newPoll);

            // Assert
            CollectionAssert.AreEquivalent(customOptions, newPoll.Options);
        }

        [TestMethod]
        public void PostRejectsPollWithBlankName()
        {
            // Act
            var response = _controller.Post(new Poll() { Name = "" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll did not have a name", error.Message);
        }

        [TestMethod]
        public void PostAssignsPollUUID()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, newPoll.UUID);
        }

        [TestMethod]
        public void PostAssignsPollManageID()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, newPoll.ManageID);
        }

        [TestMethod]
        public void PostAssignsPollManageIDDifferentFromPollId()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(newPoll.UUID, newPoll.ManageID);
        }

        [TestMethod]
        public void PostReturnsIDsOfNewPoll()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Poll responsePoll = ((ObjectContent)response.Content).Value as Poll;
            Assert.AreEqual(newPoll.UUID, responsePoll.UUID);
            Assert.AreEqual(newPoll.ManageID, responsePoll.ManageID);
        }

        [TestMethod]
        public void PostSetsIdOfNewPoll()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreEqual(UUIDs[2], newPoll.UUID);
        }

        [TestMethod]
        public void PostAddsNewPollToPolls()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            _controller.Post(newPoll);

            // Assert
            List<Poll> expectedPolls = new List<Poll>();
            expectedPolls.Add(_mainPoll);
            expectedPolls.Add(_otherPoll);
            expectedPolls.Add(newPoll);
            CollectionAssert.AreEquivalent(expectedPolls, _dummyPolls.Local);
        }

        [TestMethod]
        public void PostByIdIsAllowed()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Poll() { Name = "New Poll" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdRejectsNullPoll()
        {
            // Act
            var response = _controller.Post(UUIDs[0], null);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll is null", error.Message);
        }

        [TestMethod]
        public void PostByIdAcceptsPollWithMissingTemplateId()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Poll() { Name = "New Poll" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdAddsEmptyOptionsListToPollWithoutTemplate()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(UUIDs[0], newPoll);

            // Assert
            CollectionAssert.AreEquivalent(new List<Poll>(), newPoll.Options);
        }

        [TestMethod]
        public void PostByIdPopulatesOptionsByTemplateId()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll", TemplateId = 2 };
            var response = _controller.Post(UUIDs[0], newPoll);

            // Assert
            List<Option> expectedOptions = new List<Option>() { _redOption };
            CollectionAssert.AreEquivalent(expectedOptions, newPoll.Options);
        }

        [TestMethod]
        public void PostByIdRetainsSuppliedTemplate()
        {
            // Act
            List<Option> customOptions = new List<Option>() { _redOption };
            Poll newPoll = new Poll() { Name = "New Poll", Options = customOptions };
            var response = _controller.Post(UUIDs[0], newPoll);

            // Assert
            CollectionAssert.AreEquivalent(customOptions, newPoll.Options);
        }

        [TestMethod]
        public void PostByIdRejectsPollWithMissingName()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Poll() { });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll does not have a name", error.Message);
        }

        [TestMethod]
        public void PostByIdRejectsPollWithBlankName()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Poll() { Name = "" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll does not have a name", error.Message);
        }

        [TestMethod]
        public void PostByIdReplacesThePollWithThatId()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(UUIDs[1], newPoll);

            // Assert
            List<Poll> expectedPolls = new List<Poll>();
            expectedPolls.Add(_mainPoll);
            expectedPolls.Add(newPoll);
            CollectionAssert.AreEquivalent(expectedPolls, _dummyPolls.Local);
        }

        [TestMethod]
        public void PostByIdReturnsIdOfTheReplacedPoll()
        {
            // Act
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(UUIDs[1], newPoll);

            // Assert
            Guid responseUUID = (Guid)((ObjectContent)response.Content).Value;
            Assert.AreEqual(UUIDs[1], responseUUID);
        }

        [TestMethod]
        public void PostByIdForNewIdReturnsIdTheNewId()
        {
            // Act
            Guid newUUID = UUIDs[2];
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(newUUID, newPoll);

            // Assert
            Guid responseUUID = (Guid)((ObjectContent)response.Content).Value;
            Assert.AreEqual(newUUID, responseUUID);
        }

        [TestMethod]
        public void PostByIdForNewIdSetsUUIDOnPoll()
        {
            // Act
            Guid newUUID = UUIDs[2];
            Poll newPoll = new Poll() { Name = "New Poll" };
            var response = _controller.Post(newUUID, newPoll);

            // Assert
            Assert.AreEqual(newUUID, newPoll.UUID);
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
