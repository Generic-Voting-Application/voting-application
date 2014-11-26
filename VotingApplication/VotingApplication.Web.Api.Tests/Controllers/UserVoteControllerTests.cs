using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class UserVoteControllerTests
    {
        private UserVoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;
        private Vote _otherVote;
        private InMemoryDbSet<Vote> _dummyVotes;
        private Guid _mainUUID;
        private Guid _otherUUID;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            Session mainSession = new Session() { UUID = _mainUUID };
            Session otherSession = new Session() { UUID = _otherUUID };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            Option pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };
            User billUser = new User { Id = 3, Name = "Bill" };

            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Session> dummySessions = new InMemoryDbSet<Session>(true);

            dummyOptions.Add(burgerOption);
            dummyOptions.Add(pizzaOption);

            dummySessions.Add(mainSession);
            dummySessions.Add(otherSession);

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, SessionId = _mainUUID };
            dummyUsers.Add(bobUser);
            _dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, SessionId = _mainUUID };
            dummyUsers.Add(joeUser);
            _dummyVotes.Add(_joeVote);

            _otherVote = new Vote() { Id = 3, OptionId = 1, UserId = 1, SessionId = _otherUUID };
            _dummyVotes.Add(_otherVote);

            dummyUsers.Add(billUser);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Sessions).Returns(dummySessions);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _controller = new UserVoteController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyVotes.Local.Count; i++)
            {
                _dummyVotes.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetByUserIdReturnsVotes()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetByUserIdReturnsVotesForThatUser()
        {
            // Act
            var response = _controller.Get(1);
            List<Vote> responseVotes = ((ObjectContent)response.Content).Value as List<Vote>;

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_otherVote);
            CollectionAssert.AreEquivalent(expectedVotes, responseVotes);
        }

        [TestMethod]
        public void GetByUserIdReturns404ForUnknownUser()
        {
            // Act
            var response = _controller.Get(99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 99 not found", error.Message);
        }

        [TestMethod]
        public void GetVoteForUserByVoteIdReturnsVote()
        {
            // Act
            var response = _controller.Get(1, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetVoteForUserByVoteIdReturnsVoteMatchingId()
        {
            // Act
            var response = _controller.Get(2, 2);
            Vote responseVote = ((ObjectContent)response.Content).Value as Vote;

            // Assert
            Assert.AreEqual(_joeVote, responseVote);
        }

        [TestMethod]
        public void GetVoteForUserByVoteIdReturns404ForWrongUser()
        {
            // Act
            var response = _controller.Get(1, 2);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 2 not found", error.Message);
        }


        [TestMethod]
        public void GetVoteReturnsTheVoteWithAnOptionId()
        {
            // Act
            var response = _controller.Get(2, 2);
            Vote responseVote = ((ObjectContent)response.Content).Value as Vote;

            // Assert
            Assert.AreEqual(1, responseVote.OptionId);
        }

        [TestMethod]
        public void GetVoteTheUserWithAUserId()
        {
            // Act
            var response = _controller.Get(2, 2);
            Vote responseVote = ((ObjectContent)response.Content).Value as Vote;

            // Assert
            Assert.AreEqual(2, responseVote.UserId);
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
            var response = _controller.Post(1, new Vote());

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

        #region PUT

        [TestMethod]
        public void PutNonexistentUserIsNotAllowed()
        {
            // Act
            var response = _controller.Put(9, new List<Vote>(){ new Vote() { OptionId = 1, SessionId = _mainUUID }});

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 9 does not exist", error.Message);
        }

        [TestMethod]
        public void PutNonexistentOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new List<Vote>(){ new Vote() { OptionId = 7, SessionId = _mainUUID }});

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Option 7 does not exist", error.Message);
        }

        [TestMethod]
        public void PutMissingSessionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new List<Vote>(){ new Vote() { OptionId = 1 }});

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote does not have a session", error.Message);
        }

        [TestMethod]
        public void PutNonexistentSessionIsNotAllowed()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Put(1, new List<Vote>(){ new Vote() { OptionId = 1, SessionId = newGuid }});

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void PutMissingOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new List<Vote>(){ new Vote() });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote does not have an option", error.Message);
        }

        [TestMethod]
        public void PutWithNewVoteIfNoneExistsIsAllowed()
        {
            // Act
            var response = _controller.Put(3, new List<Vote>(){ new Vote() { OptionId = 1, SessionId = _mainUUID }});

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PutAddsANewVoteIfNoneExistsAndReturnsVoteId()
        {
            // Act
            var response = _controller.Put(3, new List<Vote>(){ new Vote() { OptionId = 1, SessionId = _mainUUID }});

            // Assert
            List<long> responseVoteIds = ((ObjectContent)response.Content).Value as List<long>;
            CollectionAssert.AreEquivalent(new List<long>() {4}, responseVoteIds);
        }

        [TestMethod]
        public void PutAddsANewVoteIfNoneExists()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, SessionId = _mainUUID };
            var response = _controller.Put(3, new List<Vote>(){ newVote });

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(_otherVote);
            expectedVotes.Add(newVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void PutReplacesCurrentVote()
        {
            // Act
            var newVote = new Vote() { OptionId = 2, SessionId = _mainUUID };
            var response = _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[0].OptionId);
        }

        [TestMethod]
        public void PutWithSessionReplacesInThatSession()
        {
            // Act
            var newVote = new Vote() { OptionId = 2, SessionId = _otherUUID };
            _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[2].OptionId);
        }

        [TestMethod]
        public void PutWithoutValueDefaultsToOne()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, SessionId = _mainUUID };
            _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.PollValue, 1);
        }

        [TestMethod]
        public void PutWithValueRetainsTheValue()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, SessionId = _mainUUID, PollValue = 35 };
            _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.PollValue, 35);
        }

        #endregion
    }
}
