﻿using System;
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
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class TokenPollVoteControllerTests
    {
        private TokenPollVoteController _controller;
        private Vote _bobVote;
        private Vote _joeVote;
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

            _bobToken = new Token { TokenGuid = Guid.NewGuid() };
            _joeToken = new Token { TokenGuid = Guid.NewGuid() };
            _otherToken = new Token { TokenGuid = Guid.NewGuid() };

            Poll mainPoll = new Poll() { UUID = _mainUUID, Expires = true, ExpiryDate = DateTime.Now.AddMinutes(30), Tokens = new List<Token>() { _bobToken, _joeToken, _otherToken } };
            Poll otherPoll = new Poll() { UUID = _otherUUID, Tokens = new List<Token>() { _otherToken } };
            Poll pointsPoll = new Poll() { UUID = _pointsUUID, PollType = PollType.Points, MaxPerVote = 5, MaxPoints = 3, Tokens = new List<Token>() { _otherToken } };
            Poll tokenPoll = new Poll() { UUID = _tokenUUID, Tokens = new List<Token>() { _validToken }, InviteOnly = true };
            Poll timedPoll = new Poll() { UUID = _timedUUID, Expires = true, ExpiryDate = DateTime.Now.AddMinutes(-30) };

            Option burgerOption = new Option { Id = 1, Name = "Burger King" };
            Option pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            Option otherOption = new Option { Id = 3, Name = "Other" };

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

            _bobVote = new Vote() { Id = 1, OptionId = 1, PollId = _mainUUID, Token = _bobToken, Option = burgerOption, Poll = mainPoll };
            _dummyVotes.Add(_bobVote);

            _joeVote = new Vote() { Id = 2, OptionId = 1, PollId = _mainUUID, Token = _joeToken };
            _dummyVotes.Add(_joeVote);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _controller = new TokenPollVoteController(mockContextFactory.Object, new VoteValidatorFactory());
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
            var response = _controller.Get(_bobToken.TokenGuid, _mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsListOfVotesForUserAndPoll()
        {
            // Act
            var response = _controller.Get(_bobToken.TokenGuid, _mainUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(1, responseVotes.Count);
            Assert.AreEqual(1, responseVotes.Single().OptionId);
        }

        [TestMethod]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(_bobToken.TokenGuid, newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void GetForValidUserAndPollWithNoVotesIsEmpty()
        {
            // Act
            var response = _controller.Get(_bobToken.TokenGuid, _otherUUID);

            // Assert
            List<VoteRequestResponseModel> responseVotes = ((ObjectContent)response.Content).Value as List<VoteRequestResponseModel>;
            Assert.AreEqual(0, responseVotes.Count);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PostRejectsVoteWithInvalidInput()
        {
             // Arrange
            _controller.ModelState.AddModelError("OptionId", "");

            // Act
            var response = _controller.Put(_bobToken.TokenGuid, _mainUUID, new List<VoteRequestModel>());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PutNonexistentOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_bobToken.TokenGuid, _mainUUID, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 99 } });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PutNonexistentPollIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_bobToken.TokenGuid, Guid.NewGuid(), new List<VoteRequestModel>() { });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [TestMethod]
        public void PutWithNewVoteIsAllowed()
        {
            // Act
            var response = _controller.Put(_otherToken.TokenGuid, _mainUUID, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1, VoteValue = 0 } });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(3, _dummyVotes.Local.Count);
        }

        [TestMethod]
        public void PutReplacesCurrentVote()
        {
            // Act
            var response = _controller.Put(_bobToken.TokenGuid, _mainUUID, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1, VoteValue = 0 } });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, _dummyVotes.Local.Count);
        }

        [TestMethod]
        public void PutWithNewVoteSetsVoteValueCorrectly()
        {
            // Act
            var response = _controller.Put(_bobToken.TokenGuid, _mainUUID, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1, VoteValue = 0 } });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, _dummyVotes.Local.Single(v => v.Token.TokenGuid == _bobToken.TokenGuid && v.PollId == _mainUUID).VoteValue);
        }

        [TestMethod]
        public void PutInvalidOptionIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_bobToken.TokenGuid, _mainUUID, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 3 } });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PutOnAnExpiredPollNotAllowed()
        {
            // Arrange
            var newVote = new Vote() { OptionId = 1, PollId = _timedUUID };

            // Act
            var response = _controller.Put(_bobToken.TokenGuid, _timedUUID, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1 } });

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual(String.Format("Poll {0} has expired", _timedUUID), error.Message);
        }

        [TestMethod]
        public void PutWithMultipleVotesIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_otherToken.TokenGuid, _mainUUID, new List<VoteRequestModel>() { new VoteRequestModel() { OptionId = 1, VoteValue = 0 }, new VoteRequestModel() { OptionId = 1, VoteValue = 0 } });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(2, _dummyVotes.Local.Count);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(Guid.NewGuid(), _mainUUID, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(Guid.NewGuid(), _mainUUID, 1, new Vote());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(Guid.NewGuid(), _mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(Guid.NewGuid(), _mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
