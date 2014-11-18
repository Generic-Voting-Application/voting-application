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
    public class SessionControllerTests
    {
        private SessionController _controller;
        private Session _mainSession;
        private Session _otherSession;
        private Guid[] UUIDs;
        private Option _redOption;
        private InMemoryDbSet<Session> _dummySessions;

        [TestInitialize]
        public void setup()
        {
            _redOption = new Option() { Name = "Red" };
            OptionSet emptyOptionSet = new OptionSet() { Id = 1 };
            OptionSet redOptionSet = new OptionSet() { Id = 2, Options = new List<Option>() { _redOption } };
            InMemoryDbSet<OptionSet> dummyOptionSets = new InMemoryDbSet<OptionSet>(true);
            dummyOptionSets.Add(emptyOptionSet);
            dummyOptionSets.Add(redOptionSet);

            UUIDs = new [] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
            _mainSession = new Session() { UUID = UUIDs[0] };
            _otherSession = new Session() { UUID = UUIDs[1] };

            _dummySessions = new InMemoryDbSet<Session>(true);
            _dummySessions.Add(_mainSession);
            _dummySessions.Add(_otherSession);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Sessions).Returns(_dummySessions);
            mockContext.Setup(a => a.OptionSets).Returns(dummyOptionSets);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _controller = new SessionController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummySessions.Local.Count; i++)
            {
                _dummySessions.Local[i].UUID = UUIDs[i];
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
        public void GetFetchesAllSessions()
        {
            // Act
            var response = _controller.Get();

            // Assert
            List<Session> responseSessions = ((ObjectContent)response.Content).Value as List<Session>;
            List<Session> expectedSessions = new List<Session>();
            expectedSessions.Add(_mainSession);
            expectedSessions.Add(_otherSession);
            CollectionAssert.AreEquivalent(expectedSessions, responseSessions);
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
        public void GetByIdOnNonexistentSessionsAreNotFound()
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
        public void GetByIdReturnsSessionWithMatchingId()
        {
            // Act
            var response = _controller.Get(UUIDs[1]);

            // Assert
            Session responseSession = ((ObjectContent)response.Content).Value as Session;
            Assert.AreEqual(_otherSession, responseSession);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new Session());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Session());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(new Session(){ Name = "New Session" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostRejectsSessionWithMissingName()
        {
            // Act
            var response = _controller.Post(new Session());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session did not have a name", error.Message);
        }

        [TestMethod]
        public void PostAcceptsSessionWithMissingOptionSet()
        {
            // Act
            var response = _controller.Post(new Session() { Name = "New Session" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostAddsEmptyOptionsListToSessionWithoutOptionSet()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(newSession);

            // Assert
            CollectionAssert.AreEquivalent(new List<Session>(), newSession.Options);
        }

        [TestMethod]
        public void PostPopulatesOptionsByOptionSetId()
        {
            // Act
            Session newSession = new Session() { Name = "New Session", OptionSetId = 2 };
            var response = _controller.Post(newSession);

            // Assert
            List<Option> expectedOptions = new List<Option>() { _redOption };
            CollectionAssert.AreEquivalent(expectedOptions, newSession.Options);
        }

        [TestMethod]
        public void PostRetainsSuppliedOptionSet()
        {
            // Act
            List<Option> customOptions = new List<Option>() { _redOption };
            Session newSession = new Session() { Name = "New Session", Options = customOptions };
            var response = _controller.Post(newSession);

            // Assert
            CollectionAssert.AreEquivalent(customOptions, newSession.Options);
        }

        [TestMethod]
        public void PostRejectsSessionWithBlankName()
        {
            // Act
            var response = _controller.Post(new Session() { Name = "" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session did not have a name", error.Message);
        }

        [TestMethod]
        public void PostAssignsSessionUUID()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            _controller.Post(newSession);

            // Assert
            Assert.AreNotEqual(Guid.Empty, newSession.UUID);
        }

        [TestMethod]
        public void PostAssignsSessionManageID()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            _controller.Post(newSession);

            // Assert
            Assert.AreNotEqual(Guid.Empty, newSession.ManageID);
        }

        [TestMethod]
        public void PostAssignsSessionManageIDDifferentFromPollId()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            _controller.Post(newSession);

            // Assert
            Assert.AreNotEqual(newSession.UUID, newSession.ManageID);
        }

        [TestMethod]
        public void PostReturnsUUIDOfNewSession()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(newSession);

            // Assert
            Guid newSessionUUID = (Guid)((ObjectContent)response.Content).Value;
            Assert.AreEqual(newSession.UUID, newSessionUUID);
        }

        [TestMethod]
        public void PostSetsIdOfNewSession()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(newSession);

            // Assert
            Assert.AreEqual(UUIDs[2], newSession.UUID);
        }

        [TestMethod]
        public void PostAddsNewSessionToSessions()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            _controller.Post(newSession);

            // Assert
            List<Session> expectedSessions = new List<Session>();
            expectedSessions.Add(_mainSession);
            expectedSessions.Add(_otherSession);
            expectedSessions.Add(newSession);
            CollectionAssert.AreEquivalent(expectedSessions, _dummySessions.Local);
        }

        [TestMethod]
        public void PostByIdIsAllowed()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Session() { Name = "New Session" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdRejectsNullSession()
        {
            // Act
            var response = _controller.Post(UUIDs[0], null);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session is null", error.Message);
        }

        [TestMethod]
        public void PostByIdAcceptsSessionWithMissingOptionSetId()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Session() { Name = "New Session" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdAddsEmptyOptionsListToSessionWithoutOptionSet()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(UUIDs[0], newSession);

            // Assert
            CollectionAssert.AreEquivalent(new List<Session>(), newSession.Options);
        }

        [TestMethod]
        public void PostByIdPopulatesOptionsByOptionSetId()
        {
            // Act
            Session newSession = new Session() { Name = "New Session", OptionSetId = 2 };
            var response = _controller.Post(UUIDs[0], newSession);

            // Assert
            List<Option> expectedOptions = new List<Option>() { _redOption };
            CollectionAssert.AreEquivalent(expectedOptions, newSession.Options);
        }

        [TestMethod]
        public void PostByIdRetainsSuppliedOptionSet()
        {
            // Act
            List<Option> customOptions = new List<Option>() { _redOption };
            Session newSession = new Session() { Name = "New Session", Options = customOptions };
            var response = _controller.Post(UUIDs[0], newSession);

            // Assert
            CollectionAssert.AreEquivalent(customOptions, newSession.Options);
        }

        [TestMethod]
        public void PostByIdRejectsSessionWithMissingName()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Session() { });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session does not have a name", error.Message);
        }

        [TestMethod]
        public void PostByIdRejectsSessionWithBlankName()
        {
            // Act
            var response = _controller.Post(UUIDs[0], new Session() { Name = "" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session does not have a name", error.Message);
        }

        [TestMethod]
        public void PostByIdReplacesTheSessionWithThatId()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(UUIDs[1], newSession);

            // Assert
            List<Session> expectedSessions = new List<Session>();
            expectedSessions.Add(_mainSession);
            expectedSessions.Add(newSession);
            CollectionAssert.AreEquivalent(expectedSessions, _dummySessions.Local);
        }

        [TestMethod]
        public void PostByIdReturnsIdOfTheReplacedSession()
        {
            // Act
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(UUIDs[1], newSession);

            // Assert
            Guid responseUUID = (Guid)((ObjectContent)response.Content).Value;
            Assert.AreEqual(UUIDs[1], responseUUID);
        }

        [TestMethod]
        public void PostByIdForNewIdReturnsIdTheNewId()
        {
            // Act
            Guid newUUID = UUIDs[2];
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(newUUID, newSession);

            // Assert
            Guid responseUUID = (Guid)((ObjectContent)response.Content).Value;
            Assert.AreEqual(newUUID, responseUUID);
        }

        [TestMethod]
        public void PostByIdForNewIdSetsUUIDOnSession()
        {
            // Act
            Guid newUUID = UUIDs[2];
            Session newSession = new Session() { Name = "New Session" };
            var response = _controller.Post(newUUID, newSession);

            // Assert
            Assert.AreEqual(newUUID, newSession.UUID);
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
