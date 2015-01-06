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
    public class UserPollVoteControllerTests
    {
        private UserPollVoteController _controller;
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

        private Token _bobToken;
        private Token _joeToken;
        private Token _otherToken;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            _pointsUUID = Guid.NewGuid();
            _tokenUUID = Guid.NewGuid();
            _timedUUID = Guid.NewGuid();
            _validToken = new Token { TokenGuid = Guid.NewGuid() };

            _bobToken = new Token { TokenGuid = Guid.NewGuid(), UserId = 1 };
            _joeToken = new Token { TokenGuid = Guid.NewGuid(), UserId = 2 };
            _otherToken = new Token { TokenGuid = Guid.NewGuid() };

            Poll mainPoll = new Poll() { UUID = _mainUUID, Expires = true, ExpiryDate = DateTime.Now.AddMinutes(30), Tokens = new List<Token>() { _bobToken, _joeToken, _otherToken } };
            Poll otherPoll = new Poll() { UUID = _otherUUID, Tokens = new List<Token>() { _otherToken } };
            Poll pointsPoll = new Poll() { UUID = _pointsUUID, VotingStrategy = "Points", MaxPerVote = 5, MaxPoints = 3, Tokens = new List<Token>() { _otherToken } };
            Poll tokenPoll = new Poll() { UUID = _tokenUUID, Tokens = new List<Token>() { _validToken }, InviteOnly = true };
            Poll timedPoll = new Poll() { UUID = _timedUUID, Expires = true, ExpiryDate = DateTime.Now.AddMinutes(-30) };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            Option pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            Option otherOption = new Option { Id = 3, Name = "Other" };
            User bobUser = new User { Id = 1, Name = "Bob", Token = _bobToken };
            User joeUser = new User { Id = 2, Name = "Joe", Token = _joeToken };
            User billUser = new User { Id = 3, Name = "Bill", Token = null };

            InMemoryDbSet<User> dummyUsers = new InMemoryDbSet<User>(true);
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            dummyOptions.Add(burgerOption);
            dummyOptions.Add(pizzaOption);
            dummyOptions.Add(otherOption);

            mainPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            otherPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            pointsPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            tokenPoll.Options = new List<Option>() { burgerOption, pizzaOption };
            timedPoll.Options = new List<Option>() { burgerOption, pizzaOption };

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(pointsPoll);
            dummyPolls.Add(tokenPoll);
            dummyPolls.Add(timedPoll);

            _bobVote = new Vote() { Id = 1, OptionId = 1, UserId = 1, PollId = _mainUUID, Token = _bobToken };
            dummyUsers.Add(bobUser);
            _dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, OptionId = 1, UserId = 2, PollId = _mainUUID, Token = _joeToken };
            dummyUsers.Add(joeUser);
            _dummyVotes.Add(_joeVote);

            dummyUsers.Add(billUser);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _controller = new UserPollVoteController(mockContextFactory.Object);
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
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get(1, _mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsListOfVotesForUserAndPoll()
        {
            // Act
            var response = _controller.Get(1, _mainUUID);

            // Assert
            List<Vote> actualVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            CollectionAssert.AreEquivalent(expectedVotes, actualVotes);
        }

        [TestMethod]
        public void GetNonexistentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(99, _mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 99 not found", error.Message);
        }

        [TestMethod]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(1, newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void GetForValidUserAndPollWithNoVotesIsEmpty()
        {
            // Act
            var response = _controller.Get(2, _otherUUID);

            // Assert
            List<Vote> actualVotes = ((ObjectContent)response.Content).Value as List<Vote>;
            List<Vote> expectedVotes = new List<Vote>();
            CollectionAssert.AreEquivalent(expectedVotes, actualVotes);
        }

        [TestMethod]
        public void GetByIdIsAllowed()
        {
            // Act
            var response = _controller.Get(1, _mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public void GetByIdForNonexistentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(99, _mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 99 not found", error.Message);
        }

        [TestMethod]
        public void GetByIdForNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(1, newGuid, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void GetByIdForNonexistentVoteIsNotFound()
        {
            // Act
            var response = _controller.Get(1, _mainUUID, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 99 not found", error.Message);
        }

        [TestMethod]
        public void GetVoteForDifferentUserIsNotFound()
        {
            // Act
            var response = _controller.Get(1, _mainUUID, 2); // Vote 2 belongs to User 2

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 2 not found", error.Message);
        }

        [TestMethod]
        public void GetVoteForDifferentPollIsNotFound()
        {
            // Act
            var response = _controller.Get(1, _mainUUID, 3); // Vote 3 belongs to Poll 2

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote 3 not found", error.Message);
        }

        [TestMethod]
        public void GetByIdFetchesIdForThatUserPoll()
        {
            // Act
            var response = _controller.Get(1, _mainUUID, 1);

            // Assert
            var responseVote = ((ObjectContent)response.Content).Value as Vote;
            Assert.AreEqual(_bobVote, responseVote);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutNonexistentUserIsNotAllowed()
        {
            // Act
            var response = _controller.Put(9, _mainUUID, new List<Vote>() { new Vote() { OptionId = 1, PollId = _mainUUID } });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("User 9 not found", error.Message);
        }

        [TestMethod]
        public void PutNonexistentOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, _mainUUID, new List<Vote>() { new Vote() { OptionId = 7, PollId = _mainUUID } });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Option 7 not found", error.Message);
        }

        [TestMethod]
        public void PutNonexistentPollIsNotAllowed()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Put(1, newGuid, new List<Vote>() { new Vote() { OptionId = 1, PollId = newGuid } });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void PutMissingOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, _mainUUID, new List<Vote>() { new Vote() });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Vote must specify an option", error.Message);
        }

        [TestMethod]
        public void PutWithNewVoteIfNoneExistsIsAllowed()
        {
            // Act
            var response = _controller.Put(3, _mainUUID, new List<Vote>() { new Vote() { OptionId = 1, PollId = _mainUUID, Token = _otherToken } });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PutAddsANewVoteIfNoneExistsAndReturnsVoteId()
        {
            // Act
            var response = _controller.Put(3, _mainUUID, new List<Vote>() { new Vote() { OptionId = 1, PollId = _mainUUID, Token = _otherToken } });

            // Assert
            List<long> responseVoteIds = ((ObjectContent)response.Content).Value as List<long>;
            CollectionAssert.AreEquivalent(new List<long>() { 3 }, responseVoteIds);
        }

        [TestMethod]
        public void PutAddsANewVoteIfNoneExists()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, PollId = _mainUUID, Token = _otherToken };
            var response = _controller.Put(3, _mainUUID, new List<Vote>() { newVote });

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(newVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void PutReplacesCurrentVote()
        {
            // Act
            var newVote = new Vote() { OptionId = 2, PollId = _mainUUID, Token = _bobToken };
            var response = _controller.Put(1, _mainUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[0].OptionId);
        }

        [TestMethod]
        public void PutWithPollReplacesInThatPoll()
        {
            // Act
            var newVote = new Vote() { OptionId = 2, PollId = _otherUUID, Token = _otherToken };
            _controller.Put(1, _otherUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[2].OptionId);
        }

        [TestMethod]
        public void PutWithoutValueDefaultsToOne()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, PollId = _mainUUID, Token = _otherToken };
            _controller.Put(1, _mainUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(newVote.PollValue, 1);
        }

        [TestMethod]
        public void PutWithValueRetainsTheValue()
        {
            // Act
            var newVote = new Vote() { OptionId = 1, PollId = _mainUUID, PollValue = 35 };
            _controller.Put(1, _mainUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(newVote.PollValue, 35);
        }

        [TestMethod]
        public void PutWithInvalidValueNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _pointsUUID, PollValue = 99, Token = _otherToken };

            // Act
            var response = _controller.Put(1, _pointsUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Invalid vote value: 99", error.Message);
        }

        [TestMethod]
        public void PutWithNoTokenOnTokenPollNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID };

            // Act
            var response = _controller.Put(1, _tokenUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("A valid token is required for poll {0}", _tokenUUID), error.Message);
        }

        [TestMethod]
        public void PutWithInvalidTokenOnTokenPollNotAllowed()
        {
            // Arrange
            Token invalidToken = new Token { TokenGuid = Guid.NewGuid() };
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID, Token = invalidToken };

            // Act
            var response = _controller.Put(1, _tokenUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("Invalid token: {0}", invalidToken.TokenGuid), error.Message);
        }

        [TestMethod]
        public void PutWithValidTokenOnTokenPollAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID, Token = _validToken };

            // Act
            _controller.Put(1, _tokenUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(newVote.OptionId, _dummyVotes.Local[0].OptionId);
        }


        [TestMethod]
        public void PutWithOnTokenPollClearsExistingVotesWithSameToken()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _tokenUUID, Token = _bobToken };
            _dummyVotes.Add(newVote);

            // Act
            _controller.Put(2, _tokenUUID, new List<Vote>() { newVote });

            // Assert
            List<Vote> expectedVotes = new List<Vote>();
            expectedVotes.Add(_bobVote);
            expectedVotes.Add(_joeVote);
            expectedVotes.Add(newVote);
            CollectionAssert.AreEquivalent(expectedVotes, _dummyVotes.Local);
        }

        [TestMethod]
        public void PutInvalidOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, _mainUUID, new List<Vote>() { new Vote { OptionId = 3, PollId = _mainUUID } });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("Option choice not valid for poll {0}", _mainUUID), error.Message);
        }

        [TestMethod]
        public void PutOnAnExpiredPollNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _timedUUID };

            // Act
            var response = _controller.Put(1, _timedUUID, new List<Vote>() { newVote });

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("Poll {0} has expired", _timedUUID), error.Message);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, _mainUUID, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, _mainUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1, _mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1, _mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
