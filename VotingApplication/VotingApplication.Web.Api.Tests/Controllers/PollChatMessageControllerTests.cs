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
        private User _bobUser;
        private InMemoryDbSet<Poll> _dummyPolls;

        [TestInitialize]
        public void setup()
        {
            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            _dummyPolls = new InMemoryDbSet<Poll>(true);

            _mainUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();

            Poll mainPoll = new Poll() { UUID = _mainUUID };
            Poll emptyPoll = new Poll() { UUID = _emptyUUID };

            _bobUser = new User { Id = 1, Name = "Bob" };

            _simpleMessage = new ChatMessage { Id = 1, User = _bobUser, Message = "Hello world" };

            mainPoll.ChatMessages = new List<ChatMessage>();
            mainPoll.ChatMessages.Add(_simpleMessage);

            dummyUsers.Add(_bobUser);

            _dummyPolls.Add(mainPoll);
            _dummyPolls.Add(emptyPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);

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
        public void PostWithInvalidPollIdNotAllowed()
        {
            // Arrange
            Guid newGuid = new Guid();
            User simpleBobUser = new User { Id = 1 };
            ChatMessage newMessage = new ChatMessage { Message = "", User = simpleBobUser };

            // Act
            var response = _controller.Post(newGuid, newMessage);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void PostWithNoMessageNotAllowed()
        {
            // Arrange
            User simpleBobUser = new User { Id = 1 };
            ChatMessage newMessage = new ChatMessage { User = simpleBobUser };

            // Act
            var response = _controller.Post(_mainUUID, newMessage);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Message text required", error.Message);
        }

        [TestMethod]
        public void PostWithInvalidUserNotAllowed()
        {
            // Arrange
            User simpleBobUser = new User { Id = 99 };
            ChatMessage newMessage = new ChatMessage { Message = "", User = simpleBobUser };

            // Act
            var response = _controller.Post(_mainUUID, newMessage);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User " + simpleBobUser.Id + " does not exist", error.Message);
        }

        [TestMethod]
        public void PostWithValidDetailsAddsMessage()
        {
            // Arrange
            User simpleBobUser = new User { Id = 1 };
            ChatMessage newMessage = new ChatMessage { Message = "", User = simpleBobUser };

            // Act
            var response = _controller.Post(_mainUUID, newMessage);

            // Assert
            List<ChatMessage> expectedMessages = new List<ChatMessage>();
            expectedMessages.Add(_simpleMessage);
            ChatMessage addedNewMessage = newMessage;
            addedNewMessage.User = _bobUser;
            expectedMessages.Add(addedNewMessage);
            
            List<ChatMessage> actualMessages = _dummyPolls.Where(p => p.UUID == _mainUUID).FirstOrDefault().ChatMessages;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEquivalent(expectedMessages, actualMessages);
        }

        [TestMethod]
        public void PostWithValidDetailsAddsMessageToMessagelessPoll()
        {
            // Arrange
            User simpleBobUser = new User { Id = 1 };
            ChatMessage newMessage = new ChatMessage { Message = "", User = simpleBobUser };

            // Act
            var response = _controller.Post(_emptyUUID, newMessage);

            // Assert
            List<ChatMessage> expectedMessages = new List<ChatMessage>();
            ChatMessage addedNewMessage = newMessage;
            addedNewMessage.User = _bobUser;
            expectedMessages.Add(addedNewMessage);

            List<ChatMessage> actualMessages = _dummyPolls.Where(p => p.UUID == _emptyUUID).FirstOrDefault().ChatMessages;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEquivalent(expectedMessages, actualMessages);
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
