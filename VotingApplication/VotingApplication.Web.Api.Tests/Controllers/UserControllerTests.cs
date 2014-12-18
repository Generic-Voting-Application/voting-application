using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;
using System;

namespace VotingApplication.Web.Api.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        private UserController _controller;
        private User _bobUser;
        private User _joeUser;
        private User _billUser;
        private InMemoryDbSet<User> dummyUsers;

        private Guid _closedPollUUID;
        private Guid _closedTokenUUID;
        private Guid _openPollUUID;
        private Guid _openTokenUUID; 
        private Guid _otherTokenUUID;

        [TestInitialize]
        public void setup()
        {
            _openTokenUUID = Guid.NewGuid();
            _closedTokenUUID = Guid.NewGuid();
            _otherTokenUUID = Guid.NewGuid();

            _closedPollUUID = Guid.NewGuid();
            _openPollUUID = Guid.NewGuid();

            Poll closedPoll = new Poll() { UUID = _closedPollUUID, Tokens = new List<Token>() { new Token() { TokenGuid = _closedTokenUUID } }, InviteOnly = true };
            Poll openPoll = new Poll() { UUID = _openPollUUID, Tokens = new List<Token>() { new Token() { TokenGuid = _openTokenUUID } }, InviteOnly = false };
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);
            dummyPolls.Add(closedPoll);
            dummyPolls.Add(openPoll);

            Token openToken = new Token() { TokenGuid = _openTokenUUID, PollId = _openPollUUID, UserId = 1 };
            Token closedToken = new Token() { TokenGuid = _closedTokenUUID, PollId = _closedPollUUID };
            Token otherToken = new Token() { TokenGuid = _otherTokenUUID, PollId = Guid.NewGuid() };
            InMemoryDbSet<Token> dummyTokens = new InMemoryDbSet<Token>(true);
            dummyTokens.Add(openToken);
            dummyTokens.Add(closedToken);
            dummyTokens.Add(otherToken);

            _bobUser = new User { Id = 1, Name = "Bob", PollId = _openPollUUID, TokenId = _openTokenUUID };
            _joeUser = new User { Id = 2, Name = "Joe", PollId = _openPollUUID };
            _billUser = new User { Name = "Bill", PollId = _openPollUUID };
            dummyUsers = new InMemoryDbSet<User>(true);
            dummyUsers.Add(_bobUser);
            dummyUsers.Add(_joeUser);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.Tokens).Returns(dummyTokens);
            mockContext.Setup(a => a.SaveChanges()).Callback(this.SaveChanges);

            _controller = new UserController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < dummyUsers.Local.Count; i++)
            {
                dummyUsers.Local[i].Id = (long)i+1;
            }
        }

        #region GET
        [TestMethod]
        public void GetReturnsAllUsers()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsNonNullUsers()
        {
            // Act
            var response = _controller.Get();
            List<User> responseUsers = ((ObjectContent)response.Content).Value as List<User>;

            // Assert
            Assert.IsNotNull(responseUsers);
        }

        [TestMethod]
        public void GetReturnsUsersFromTheDatabase()
        {
            // Act
            var response = _controller.Get();
            List<User> responseUsers = ((ObjectContent)response.Content).Value as List<User>;

            // Assert
            List<User> expectedUsers = new List<User>();
            expectedUsers.Add(_bobUser);
            expectedUsers.Add(_joeUser);
            Debug.WriteLine(responseUsers);
            CollectionAssert.AreEquivalent(expectedUsers, responseUsers);
        }

        [TestMethod]
        public void GetWithIdFindsUserWithId()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsUserWithIdFromTheDatabase()
        {
            // Act
            var response = _controller.Get(2);
            User responseUser = ((ObjectContent)response.Content).Value as User;

            // Assert
            Assert.AreEqual(_joeUser.Id, responseUser.Id);
            Assert.AreEqual(_joeUser.Name, responseUser.Name);
        }

        [TestMethod]
        public void GetWithIdReturnsErrorCode404ForUnknownUserID()
        {
            // Act
            var response = _controller.Get(3);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsNullUserForUnknownUserID()
        {
            // Act
            var response = _controller.Get(3);
            User responseUser = ((ObjectContent)response.Content).Value as User;

            // Assert
            Assert.IsNull(responseUser);
        } 
        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(new List<object>());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, new User());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new User());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutReturnsOK()
        {
            // Act
            var response = _controller.Put(_billUser);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PutNewUserAddsNewUserToUserList()
        {
            // Act
            var response = _controller.Put(_billUser);

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            expectedList.Add(_billUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PutNewUserAssignsNewUserAUniqueId()
        {
            // Act
            var response = _controller.Put(_billUser);

            // Assert
            Assert.AreEqual(3, dummyUsers.Local[2].Id);
        }

        [TestMethod]
        public void PutNewUserWithExistingUsernameIsAllowed()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Bob", PollId = _openPollUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PutNewUserWithExistingTokenReturnsExistingUserToken()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Bob", PollId = _openPollUUID, TokenId = _openTokenUUID });

            // Assert
            Token responseToken = ((ObjectContent)response.Content).Value as Token;
            Assert.AreEqual(1, responseToken.UserId);
        }

        [TestMethod]
        public void PutNewUserWithExistingUsernameDoesNotChangeUserList()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Bob" });

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PutNewUserWithEmptyUsernameIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new User() { Name = String.Empty });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PutNewUserWithEmptyUsernameDoesNotChangeUserList()
        {
            // Act
            var response = _controller.Put(new User() { Name = String.Empty });

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PutNewUserWithMissingUsernameIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new User());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PutNewUserWithMissingUsernameDoesNotChangeUserList()
        {
            // Act
            var response = _controller.Put(new User());

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PutNewUserWithInvalidUsernameIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new User() { Name = "<span>" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PutNewUserWithInvalidUsernameDoesNotChangeUserList()
        {
            // Act
            var response = _controller.Put(new User() { Name = "<span>" });

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PutUserInClosedPollRejectsMissingToken()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", PollId = _closedPollUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User missing a valid token for this poll", error.Message);
        }

        [TestMethod]
        public void PutUserInClosedPollRejectsMissingPollId()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", TokenId = _closedTokenUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User missing a poll", error.Message);
        }

        [TestMethod]
        public void PutUserInClosedPollRejectsTokenPollMismatch()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", TokenId = _otherTokenUUID, PollId = _closedPollUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User missing a valid token for this poll", error.Message);
        }

        [TestMethod]
        public void PutUserInClosedPollAcceptsMatchingTokenForPoll()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", TokenId = _closedTokenUUID, PollId = _closedPollUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PutUserInOpenPollRejectsMissingPollId()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", TokenId = _openTokenUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User missing a poll", error.Message);
        }

        [TestMethod]
        public void PutUserInOpenPollAcceptsMissingToken()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", PollId = _openPollUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PutUserInOpenPollGeneratesNewTokenForMissingToken()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", PollId = _openPollUUID });

            // Assert
            Token responseToken = ((ObjectContent)response.Content).Value as Token;
            Assert.AreNotEqual(Guid.Empty, responseToken.TokenGuid);
            Assert.AreNotEqual(_closedTokenUUID, responseToken.TokenGuid);
            Assert.AreNotEqual(_openTokenUUID, responseToken.TokenGuid);
            Assert.AreNotEqual(_otherTokenUUID, responseToken.TokenGuid);
        }

        [TestMethod]
        public void PutUserInOpenPollPopulatesTokenWithUserIdAndPollId()
        {
            // Act
            User newUser = new User() { Name = "Name", PollId = _openPollUUID };
            var response = _controller.Put(newUser);

            // Assert
            Token responseToken = ((ObjectContent)response.Content).Value as Token;
            Assert.AreEqual(newUser.Id, responseToken.UserId);
            Assert.AreEqual(_openPollUUID, responseToken.PollId);
        }

        [TestMethod]
        public void PutUserInOpenPollRejectsPollTokenMismatch()
        {
            // Act
            var response = _controller.Put(new User() { Name = "Name", TokenId = _otherTokenUUID, PollId = _openPollUUID });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User missing a valid token for this poll", error.Message);
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
