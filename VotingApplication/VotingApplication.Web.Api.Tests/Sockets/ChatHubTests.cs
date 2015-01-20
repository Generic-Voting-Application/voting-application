using FakeDbSet;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Sockets;

namespace VotingApplication.Web.Api.Tests.Sockets
{
    [TestClass]
    public class ChatHubTests
    {
        #region Setup

        private ChatHub _hub;
        private Mock<ChatHub> _hubMock;
        private Mock<IGroupManager> _groupsMock;
        private Mock<IHubCallerConnectionContext<dynamic>> _clientsMock;

        private Guid _mainUUID;
        private Guid _emptyUUID;
        private ChatMessage _simpleMessage;
        private User _bobUser;
        private InMemoryDbSet<Poll> _dummyPolls;

        private dynamic _group;
        private ChatMessage _broadcastMessage;

        private dynamic _caller;
        private List<ChatMessage> _broadcastMessages;
        private string _reportError;

        private const string ConnectionId = "1023";

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

            // Create the test target
            _hubMock = new Mock<ChatHub>(mockContextFactory.Object) { CallBase = true };
            _hubMock.SetupGet(h => h.ConnectionId).Returns(ConnectionId);
            _hub = _hubMock.Object;

            // Give it a mock groups collection
            _groupsMock = new Mock<IGroupManager>();
            _hub.Groups = _groupsMock.Object;

            // Give it a mock client connection context
            _clientsMock = new Mock<IHubCallerConnectionContext<dynamic>>();
            _hub.Clients = _clientsMock.Object;

            // Setup mocks for the outbound messages
            _group = new ExpandoObject();
            _group.broadcastMessage = new Action<ChatMessage>(message => {
                _broadcastMessage = message;
            });
            _clientsMock.Setup(c => c.Group(_mainUUID.ToString())).Returns((ExpandoObject)_group);
            _clientsMock.Setup(c => c.Group(_emptyUUID.ToString())).Returns((ExpandoObject)_group);

            _caller = new ExpandoObject();
            _caller.broadcastMessages = new Action<List<ChatMessage>>(list => {
                _broadcastMessages = list;
            });
            _caller.reportError = new Action<string>(message => {
                _reportError = message;
            });
            _clientsMock.Setup(c => c.Caller).Returns((ExpandoObject)_caller);
        }

        #endregion

        #region JoinPoll

        [TestMethod]
        public void JoinPoll_with_NonexistentPoll_expect_SendNotFoundError()
        {
            // Arrange
            Guid newGuid = Guid.NewGuid();

            // Act
            _hub.JoinPoll(newGuid);

            // Assert
            Assert.AreEqual(string.Format("Poll {0} not found", newGuid), _reportError);
            Assert.IsNull(_broadcastMessages);
        }

        [TestMethod]
        public void JoinPoll_with_PollHasMessages_expect_SendMessages()
        {
            // Act
            _hub.JoinPoll(_mainUUID);

            // Assert
            CollectionAssert.AreEquivalent(new List<ChatMessage> { _simpleMessage }, _broadcastMessages);

            // Should have joined the group
            _groupsMock.Verify(g => g.Add(ConnectionId, _mainUUID.ToString()));
        }

        [TestMethod]
        public void JoinPoll_with_PollNoMessages_expect_SendEmptyList()
        {
            _hub.JoinPoll(_emptyUUID);

            // Assert
            CollectionAssert.AreEquivalent(new List<ChatMessage>(), _broadcastMessages);

            // Should have joined the group
            _groupsMock.Verify(g => g.Add(ConnectionId, _emptyUUID.ToString()));
        }
        #endregion

        #region SendMessage

        [TestMethod]
        public void SendMessage_with_InvalidPollId_expect_SendError()
        {
            // Arrange
            Guid newGuid = new Guid();

            // Act
            _hub.SendMessage(newGuid, 1, "A message");

            // Assert
            Assert.AreEqual(string.Format("Poll {0} not found", newGuid), _reportError);
            Assert.IsNull(_broadcastMessage);
        }

        [TestMethod]
        public void SendMessage_with_NoMessage_expect_SendError()
        {
            // Act
            _hub.SendMessage(_mainUUID, 1, "");

            // Assert
            Assert.AreEqual("Message text required", _reportError);
            Assert.IsNull(_broadcastMessage);
        }

        [TestMethod]
        public void SendMessage_with_InvalidUser_expect_SendError()
        {
            // Act
            _hub.SendMessage(_mainUUID, 99, "Hello");

            // Assert
            Assert.AreEqual("User 99 not found", _reportError);
            Assert.IsNull(_broadcastMessage);
        }

        [TestMethod]
        public void SendMessage_with_ValidDetails_expect_AddsMessage()
        {
            // Act
            _hub.SendMessage(_mainUUID, 1, "My Message");

            // Assert
            Assert.IsNull(_reportError);

            var actualMessages = _dummyPolls.Where(p => p.UUID == _mainUUID).Single().ChatMessages;
            Assert.AreEqual(2, actualMessages.Count);

            var newMessage = actualMessages[1];
            Assert.AreEqual("My Message", newMessage.Message);
            Assert.AreEqual(1, newMessage.User.Id);
            Assert.AreEqual("Bob", newMessage.User.Name);

            Assert.IsTrue(DateTimeOffset.Now.Subtract(newMessage.Timestamp) < TimeSpan.FromSeconds(1),
                "Message timestamp should be set");
        }

        [TestMethod]
        public void SendMessage_with_ValidDetails_expect_BroadcastMessage()
        {
            _hub.SendMessage(_mainUUID, 1, "My Message");

            // Assert
            Assert.IsNull(_reportError);

            Assert.IsNotNull(_broadcastMessage);
            Assert.AreEqual("My Message", _broadcastMessage.Message);
            Assert.AreEqual(1, _broadcastMessage.User.Id);
            Assert.AreEqual("Bob", _broadcastMessage.User.Name);

            Assert.IsTrue(DateTimeOffset.Now.Subtract(_broadcastMessage.Timestamp) < TimeSpan.FromSeconds(1),
                "Message timestamp should be set");

        }

        #endregion
    }
}
