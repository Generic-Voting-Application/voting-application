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
    public class SessionOptionControllerTests
    {
        private SessionOptionController _controller;
        private Guid _mainUUID;
        private Guid _emptyUUID;
        private Option _burgerOption;
        private Option _pizzaOption;
        private Vote _burgerVote;
        private Session _mainSession;
        private InMemoryDbSet<Option> _dummyOptions;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();

            _burgerOption = new Option { Id = 1, Name = "Burger King" };
            _pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            _dummyOptions = new InMemoryDbSet<Option>(true);
            _dummyOptions.Add(_burgerOption);
            _dummyOptions.Add(_pizzaOption);

            _burgerVote = new Vote() { Id = 1, SessionId = _mainUUID, OptionId = 1 };
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            _dummyVotes.Add(_burgerVote);

            InMemoryDbSet<Session> dummySessions = new InMemoryDbSet<Session>(true);
            _mainSession = new Session() { UUID = _mainUUID, Options = new List<Option>() { _burgerOption, _pizzaOption } };
            Session emptySession = new Session() { UUID = _emptyUUID, Options = new List<Option>() };
            dummySessions.Add(_mainSession);
            dummySessions.Add(emptySession);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Options).Returns(_dummyOptions);
            mockContext.Setup(a => a.Sessions).Returns(dummySessions);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);

            _controller = new SessionOptionController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyOptions.Count(); i++)
            {
                _dummyOptions.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithNonexistentSessionIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void GetWithEmptySessionReturnsEmptyOptionList()
        {
            // Act
            var response = _controller.Get(_emptyUUID);
            
            // Assert
            List<Option> expectedOptions = new List<Option>();
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;
            CollectionAssert.AreEquivalent(expectedOptions, responseOptions);
        }

        [TestMethod]
        public void GetWithPopulatedSessionReturnsOptionsForThatSession()
        {
            // Act
            var response = _controller.Get(_mainUUID);
            
            // Assert
            List<Option> expectedOptions = new List<Option>() { _burgerOption, _pizzaOption };
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;
            CollectionAssert.AreEquivalent(expectedOptions, responseOptions);
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

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_mainUUID, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_mainUUID, 1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, new Option() { Name = "Abc", Description = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostWithoutNameIsRejected()
        {
            // Act
            var response = _controller.Post(_mainUUID, new Option() { Description = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Cannot create an option with a non-empty name", error.Message);
        }

        [TestMethod]
        public void PostWithEmptyNameIsRejected()
        {
            // Act
            var response = _controller.Post(_mainUUID, new Option() { Name = "", Description = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Cannot create an option with a non-empty name", error.Message);
        }


        [TestMethod]
        public void PostWithoutDescriptionIsAccepted()
        {
            // Act
            var response = _controller.Post(_mainUUID, new Option() { Name = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostWithoutMoreInfoIsAccepted()
        {
            // Act
            var response = _controller.Post(_mainUUID, new Option() { Name = "Abc", Description = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostAddsOptionToOptionList()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
            _controller.Post(_mainUUID, newOption);

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_burgerOption);
            expectedOptions.Add(_pizzaOption);
            expectedOptions.Add(newOption);
            CollectionAssert.AreEquivalent(expectedOptions, _dummyOptions.Local);
        }

        [TestMethod]
        public void PostReturnsIdOfNewOption()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
            var response = _controller.Post(_mainUUID, newOption);

            // Assert
            long optionId = (long)((ObjectContent)response.Content).Value;
            Assert.AreEqual(3, optionId);
        }

        [TestMethod]
        public void PostSetsIdOfNewOption()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
            _controller.Post(_mainUUID, newOption);

            // Assert
            Assert.AreEqual(3, newOption.Id);
        }


        [TestMethod]
        public void PostReturnsNotFoundForMissingSession()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            Option newOption = new Option() { Name = "Bella Vista" };
            var response = _controller.Post(newGuid, newOption);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void PostAddsOptionToSession()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
            _controller.Post(_mainUUID, newOption);

            // Assert
            List<Option> expectedOptions = new List<Option>() { _burgerOption, _pizzaOption, newOption };
            CollectionAssert.AreEquivalent(expectedOptions, _mainSession.Options);
        }

        [TestMethod]
        public void PostAddsSessionToOptions()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
            _controller.Post(_mainUUID, newOption);

            // Assert
            List<Session> expectedSessions = new List<Session>() { _mainSession };
            CollectionAssert.AreEquivalent(expectedSessions, newOption.Sessions);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, 1, new Option());

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
        public void DeleteByIdIsAllowed()
        {
            // Act
            var response = _controller.Delete(_mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdRemovesOptionWithMatchingIdFromSession()
        {
            // Act
            _controller.Delete(_mainUUID, 1);

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_pizzaOption);
            CollectionAssert.AreEquivalent(expectedOptions, _mainSession.Options);
        }

        [TestMethod]
        public void DeleteByIdDoesNotRemoveOptionFromGlobalOptions()
        {
            // Act
            _controller.Delete(_mainUUID, 1);

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_burgerOption);
            expectedOptions.Add(_pizzaOption);
            CollectionAssert.AreEquivalent(expectedOptions, _dummyOptions.Local);
        }

        [TestMethod]
        public void DeleteByIdIsAllowedIfNoOptionMatchesId()
        {
            // Act
            var response = _controller.Delete(_mainUUID, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsIsNotAllowedForMissingSessionUUID()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Delete(newGuid, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void DeleteOptionRemovesRelevantVotes()
        {
            // Act
            _controller.Delete(_mainUUID, 1);

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>(), _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteOptionDoesNotRemoveOtherVotes()
        {
            // Act
            _controller.Delete(_mainUUID, 2);

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>() { _burgerVote }, _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteOptionOnlyDeletesFromCurrentSession()
        {
            // Act
            _controller.Delete(_emptyUUID, 1);

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>() { _burgerVote }, _dummyVotes.Local);
        }

        #endregion
    }
}
