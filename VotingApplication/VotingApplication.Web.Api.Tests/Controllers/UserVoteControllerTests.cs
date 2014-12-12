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
        private Guid _pointsUUID;
        private Guid _tokenUUID;
        private Guid _timedUUID;
        private Token _validToken;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            _pointsUUID = Guid.NewGuid();
            _tokenUUID = Guid.NewGuid();
            _timedUUID = Guid.NewGuid();
            _validToken = new Token { TokenGuid = Guid.NewGuid() };

            Poll mainPoll = new Poll() { UUID = _mainUUID };
            Poll otherPoll = new Poll() { UUID = _otherUUID };
            Poll pointsPoll = new Poll() { UUID = _pointsUUID };
            Poll tokenPoll = new Poll() { UUID = _tokenUUID };
            Poll timedPoll = new Poll() { UUID = _timedUUID };

            pointsPoll.VotingStrategy = "Points";
            pointsPoll.MaxPerVote = 5;
            pointsPoll.MaxPoints = 3;

            tokenPoll.Tokens = new List<Token>();
            tokenPoll.Tokens.Add(_validToken);
            tokenPoll.InviteOnly = true;

            timedPoll.Expires = true;
            timedPoll.ExpiryDate = DateTime.Now.AddMinutes(-30);

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            Option pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            Option otherOption = new Option { Id = 3, Name = "Other" };
            User bobUser = new User { Id = 1, Name = "Bob" };
            User joeUser = new User { Id = 2, Name = "Joe" };
            User billUser = new User { Id = 3, Name = "Bill" };

            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            dummyOptions.Add(burgerOption);
            dummyOptions.Add(pizzaOption);
            dummyOptions.Add(otherOption);

            mainPoll.Options = new List<Option>();
            mainPoll.Options.Add(burgerOption);
            mainPoll.Options.Add(pizzaOption);

            otherPoll.Options = new List<Option>();
            otherPoll.Options.Add(burgerOption);
            otherPoll.Options.Add(pizzaOption);

            pointsPoll.Options = new List<Option>();
            pointsPoll.Options.Add(burgerOption);
            pointsPoll.Options.Add(pizzaOption);

            tokenPoll.Options = new List<Option>();
            tokenPoll.Options.Add(burgerOption);
            tokenPoll.Options.Add(pizzaOption);

            timedPoll.Options = new List<Option>();
            timedPoll.Options.Add(burgerOption);
            timedPoll.Options.Add(pizzaOption);

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(pointsPoll);
            dummyPolls.Add(tokenPoll);
            dummyPolls.Add(timedPoll);

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, PollId = _mainUUID };
            dummyUsers.Add(bobUser);
            _dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, PollId = _mainUUID };
            dummyUsers.Add(joeUser);
            _dummyVotes.Add(_joeVote);

            _otherVote = new Vote() { Id = 3, OptionId = 1, UserId = 1, PollId = _otherUUID };
            _dummyVotes.Add(_otherVote);

            dummyUsers.Add(billUser);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
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
            var response = _controller.Put(9, new List<Vote>() { new Vote() { OptionId = 1, PollId = _mainUUID } });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 9 does not exist", error.Message);
        }

        [TestMethod]
        public void PutNonexistentOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new List<Vote>() { new Vote() { OptionId = 7, PollId = _mainUUID } });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Option 7 does not exist", error.Message);
        }

        [TestMethod]
        public void PutMissingPollIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new List<Vote>(){ new Vote() { OptionId = 1 }});

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote does not have a poll", error.Message);
        }

        [TestMethod]
        public void PutNonexistentPollIsNotAllowed()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Put(1, new List<Vote>() { new Vote() { OptionId = 1, PollId = newGuid } });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " does not exist", error.Message);
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
            var response = _controller.Put(3, new List<Vote>() { new Vote() { OptionId = 1, PollId = _mainUUID } });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PutAddsANewVoteIfNoneExistsAndReturnsVoteId()
        {
            // Act
            var response = _controller.Put(3, new List<Vote>() { new Vote() { OptionId = 1, PollId = _mainUUID } });

            // Assert
            List<long> responseVoteIds = ((ObjectContent)response.Content).Value as List<long>;
            CollectionAssert.AreEquivalent(new List<long>() {4}, responseVoteIds);
        }

        [TestMethod]
        public void PutAddsANewVoteIfNoneExists()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, PollId = _mainUUID };
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
            var newVote = new Vote() { OptionId = 2, PollId = _mainUUID };
            var response = _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[0].OptionId);
        }

        [TestMethod]
        public void PutWithPollReplacesInThatPoll()
        {
            // Act
            var newVote = new Vote() { OptionId = 2, PollId = _otherUUID };
            _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[2].OptionId);
        }

        [TestMethod]
        public void PutWithoutValueDefaultsToOne()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, PollId = _mainUUID };
            _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.PollValue, 1);
        }

        [TestMethod]
        public void PutWithValueRetainsTheValue()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, PollId = _mainUUID, PollValue = 35 };
            _controller.Put(1, new List<Vote>(){ newVote });

            // Assert
            Assert.AreEqual(newVote.PollValue, 35);
        }

        [TestMethod]
        public void PutWithInvalidValueNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _pointsUUID, PollValue = 99 };

            // Act
            var response = _controller.Put(1, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Invalid vote value: 99", error.Message);
        }

        [TestMethod]
        public void PutWithNoTokenOnTokenPollNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID};

            // Act
            var response = _controller.Put(1, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Token required for this poll", error.Message);
        }

        [TestMethod]
        public void PutWithInvalidTokenOnTokenPollNotAllowed()
        {
            // Arrange
            Token invalidToken = new Token { TokenGuid = Guid.NewGuid() };
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID, Token = invalidToken };

            // Act
            var response = _controller.Put(1, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("Invalid token: {0}", invalidToken), error.Message);
        }

        [TestMethod]
        public void PutWithValidTokenOnTokenPollAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID, Token = _validToken };

            // Act
            _controller.Put(1, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[0].OptionId);
        }


        [TestMethod]
        public void PutWithOnTokenPollClearsExistingVotesWithSameToken()
        {
            // Arrange
            _dummyVotes.Add(new Vote() { Id = 4, OptionId = 1, UserId = 1, PollId = _tokenUUID, Token = _validToken });
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID, Token = _validToken };

            // Act
            _controller.Put(2, new List<Vote>() { newVote });

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(_otherVote);
            expectedVotes.Add(newVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void PutInvalidOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new List<Vote>() { new Vote { OptionId = 3, PollId = _mainUUID} });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("Option not valid for poll {0}", _mainUUID), error.Message);
        }

        [TestMethod]
        public void PutOnAnExpiredPollNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _timedUUID };

            // Act
            var response = _controller.Put(1, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("Poll {0} has expired", _timedUUID), error.Message);
        }

        #endregion
    }
}
