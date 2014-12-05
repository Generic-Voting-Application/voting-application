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
    public class PollChatMessageVoteControllerTests
    {
        private PollChatMessageController _controller;
        private Guid _mainUUID;
        private Guid _emptyUUID;
        private ChatMessage _simpleMessage;

        [TestInitialize]
        public void setup()
        {
            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            _mainUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();

            Poll mainPoll = new Poll() { UUID = _mainUUID };
            Poll emptyPoll = new Poll() { UUID = _emptyUUID };

            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };

            _simpleMessage = new ChatMessage { Id = 1, User = bobUser, Message = "Hello world" };

            mainPoll.ChatMessages = new List<ChatMessage>();
            mainPoll.ChatMessages.Add(_simpleMessage);

            dummyUsers.Add(bobUser);
            dummyUsers.Add(joeUser);

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(emptyPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);

            _controller = new PollChatMessageController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetIsNotAllowed()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Cannot use GET on this controller", error.Message);

        }

        [TestMethod]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void GetReturnsMessagesForThatPoll()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            List<ChatMessage> expectedMessages = new List<ChatMessage>();
            expectedMessages.Add(_simpleMessage);
            List<ChatMessage> responseMessages = ((ObjectContent)response.Content).Value as List<ChatMessage>;
            CollectionAssert.AreEquivalent(expectedMessages, responseMessages);
        }

        [TestMethod]
        public void GetOnMessagelessPollReturnsEmptyList()
        {
            // Act
            var response = _controller.Get(_emptyUUID);

            // Assert
            List<ChatMessage> expectedMessages = new List<ChatMessage>();
            List<ChatMessage> responseMessages = ((ObjectContent)response.Content).Value as List<ChatMessage>;
            CollectionAssert.AreEquivalent(expectedMessages, responseMessages);
        }
        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            //var response = _controller.Post(_mainUUID, new Vote());

            // Assert
           // Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            //var response = _controller.Post(_mainUUID, 1, new Vote());

            // Assert
            //Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
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
