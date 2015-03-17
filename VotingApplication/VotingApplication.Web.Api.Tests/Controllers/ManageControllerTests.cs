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
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Services;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class ManageControllerTests
    {
        private ManageController _controller;
        private Guid _manageMainUUID;
        private Guid _manageEmptyUUID;
        private Option _burgerOption;
        private Option _pizzaOption;
        private Vote _burgerVote;
        private Poll _mainPoll;
        private InMemoryDbSet<Option> _dummyOptions;
        private InMemoryDbSet<Vote> _dummyVotes;
        private InMemoryDbSet<Poll> _dummyPolls;
        private InMemoryDbSet<Token> _dummyTokens;

        [TestInitialize]
        public void setup()
        {
            Guid mainUUID = Guid.NewGuid();
            Guid emptyUUID = Guid.NewGuid();
            _manageMainUUID = Guid.NewGuid();
            _manageEmptyUUID = Guid.NewGuid();

            _burgerOption = new Option { Id = 1, Name = "Burger King" };
            _pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            _dummyOptions = new InMemoryDbSet<Option>(true);
            _dummyOptions.Add(_burgerOption);
            _dummyOptions.Add(_pizzaOption);

            _burgerVote = new Vote() { Id = 1, PollId = mainUUID, OptionId = 1 };
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            _dummyVotes.Add(_burgerVote);

            _dummyPolls = new InMemoryDbSet<Poll>(true);
            _mainPoll = new Poll() { UUID = mainUUID, ManageId = _manageMainUUID, Options = new List<Option>() { _burgerOption, _pizzaOption }, Tokens = new List<Token>() };
            Poll emptyPoll = new Poll() { UUID = emptyUUID, ManageId = _manageEmptyUUID, Options = new List<Option>(), Tokens = new List<Token>() };
            _dummyPolls.Add(_mainPoll);
            _dummyPolls.Add(emptyPoll);

            _dummyTokens = new InMemoryDbSet<Token>(true);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Options).Returns(_dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Tokens).Returns(_dummyTokens);

            var mockMail = new Mock<IMailSender>();

            _controller = new ManageController(mockContextFactory.Object, mockMail.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyOptions.Count(); i++)
            {
                _dummyOptions.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            _controller.Get(_manageMainUUID);
        }


        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetWithNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);
        }

        [TestMethod]
        public void GetWithEmptyPollReturnsEmptyOptionList()
        {
            // Act
            var response = _controller.Get(_manageEmptyUUID);

            // Assert
            Assert.AreEqual(0, response.Options.Count);
        }

        [TestMethod]
        public void GetWithPopulatedPollReturnsOptionsForThatPoll()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            Assert.AreEqual(2, response.Options.Count);
            CollectionAssert.AreEqual(new string[] { "Burger King", "Pizza Hut" }, response.Options.Select(r => r.Name).ToArray());
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsAllowed()
        {
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Voters = new List<Token>()
            };

            // Act
            _controller.Put(_manageMainUUID, request);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PutReturnsNotFoundForMissingPoll()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Put(newGuid, new ManagePollUpdateRequest());
        }

        [TestMethod]
        public void PutOverwritesExistingOptionsOnAPoll()
        {
            List<Option> newOptions = new List<Option>() { new Option() { Name = "Test", Description = "Abc" } };
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Options = newOptions,
                Voters = new List<Token>()
            };
            _controller.Put(_manageMainUUID, request);

            // Assert
            CollectionAssert.AreEquivalent(_mainPoll.Options, newOptions);
            Assert.AreEqual(0, _mainPoll.Options[0].Id);
        }

        [TestMethod]
        public void PutRetainsExistingOptionsOnAPollIfIdsMatch()
        {
            List<Option> newOptions = new List<Option>() { new Option() { Name = "Test", Description = "Abc" }, _burgerOption };
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Options = newOptions,
                Voters = new List<Token>()
            };
            _controller.Put(_manageMainUUID, request);

            // Assert
            Assert.AreEqual(_burgerOption, _mainPoll.Options.Find(o => o.Id == _burgerOption.Id));
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutInvalidOptionIsRejected()
        {
            // Act
            List<Option> invalidOptions = new List<Option>() { new Option() { Description = "Abc" } };
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Options = invalidOptions,
                Voters = new List<Token>()
            };
            _controller.Put(_manageMainUUID, request);
        }

        [TestMethod]
        public void PutClearsOptionsIfNoneAreGiven()
        {
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Name = "Test",
                VotingStrategy = PollType.Basic.ToString(),
                Voters = new List<Token>()
            };
            _controller.Put(_manageMainUUID, request);

            // Assert
            Assert.AreEqual(0, _mainPoll.Options.Count);
        }

        [TestMethod]
        public void PutWithNewEmailsGeneratesTokensForEmails()
        {
            // Arrange
            Token newToken = new Token() {Email = "a@b.c"};
            List<Token> newEmailTokens = new List<Token>() {newToken};
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Voters = newEmailTokens
            };

            // Act
            _controller.Put(_manageMainUUID, request);

            // Assert
            Assert.AreNotEqual(Guid.Empty, newToken.TokenGuid);
        }

        [TestMethod]
        public void PutWithNewEmailAddsToTokenListOfPoll()
        {
            // Arrange
            Token newToken = new Token() { Email = "a@b.c" };
            List<Token> newEmailTokens = new List<Token>() { newToken };
            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Voters = newEmailTokens
            };

            // Act
            _controller.Put(_manageMainUUID, request);

            // Assert
            CollectionAssert.AreEquivalent(newEmailTokens, _mainPoll.Tokens);
        }

        [TestMethod]
        public void PutWithExistingTokenDoesNotModifyToken()
        {
            // Arrange
            Token existingToken = new Token() { Email = "a@b.c", TokenGuid = Guid.NewGuid(), Id = 1 };
            List<Token> emailTokens = new List<Token>() { existingToken };
            _mainPoll.Tokens = emailTokens;

            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Voters = emailTokens
            };

            // Act
            _controller.Put(_manageMainUUID, request);

            // Assert
            CollectionAssert.AreEquivalent(emailTokens, _mainPoll.Tokens);
        }

        [TestMethod]
        public void PutWithEmptyTokenListClearsObsoleteTokens()
        {
            // Arrange
            Token existingToken = new Token() { Email = "a@b.c", TokenGuid = Guid.NewGuid(), Id = 1 };
            Token obsoleteToken = new Token() { Email = "d@e.f", TokenGuid = Guid.NewGuid(), Id = 2 };
            Token newToken = new Token() { Email = "g@h.i" };
            List<Token> emailTokens = new List<Token>() { existingToken, obsoleteToken };
            _mainPoll.Tokens = emailTokens;

            ManagePollUpdateRequest request = new ManagePollUpdateRequest
            {
                Voters = new List<Token>() { existingToken, newToken }
            };

            // Act
            _controller.Put(_manageMainUUID, request);

            // Assert
            List<Token> expectedTokens = new List<Token>() { existingToken, newToken };
            CollectionAssert.AreEquivalent(expectedTokens, _mainPoll.Tokens);
        }

        #endregion
    }
}
