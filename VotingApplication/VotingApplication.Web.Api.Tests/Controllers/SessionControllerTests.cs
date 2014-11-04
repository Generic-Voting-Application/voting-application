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
        private InMemoryDbSet<Session> _dummySessions;

        [TestInitialize]
        public void setup()
        {
            _mainSession = new Session() { Id = 1 };
            _otherSession = new Session() { Id = 2 };

            _dummySessions = new InMemoryDbSet<Session>(true);
            _dummySessions.Add(_mainSession);
            _dummySessions.Add(_otherSession);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Sessions).Returns(_dummySessions);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _controller = new SessionController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummySessions.Local.Count; i++)
            {
                _dummySessions.Local[i].Id = (long)i + 1;
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
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetByIdOnNonexistentSessionsAreNotFound()
        {
            // Act
            var response = _controller.Get(99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session 99 does not exist", error.Message);
        }

        [TestMethod]
        public void GetByIdReturnsSessionWithMatchingId()
        {
            // Act
            var response = _controller.Get(2);

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
            Assert.AreEqual(3, newSession.Id);
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
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, new Session());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
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
