using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class PollOptionControllerTests
    {
        private PollOptionController _controller;
        private Guid _mainUUID;
        private Guid _emptyUUID;
        private Option _burgerOption;
        private Option _pizzaOption;
        private Vote _burgerVote;
        private Poll _mainPoll;
        private Poll _emptyPoll;
        private InMemoryDbSet<Option> _dummyOptions;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            _mainUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();

            _burgerOption = new Option { Id = 1, Name = "Burger King" };
            _pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            _dummyOptions = new InMemoryDbSet<Option>(true);
            _dummyOptions.Add(_burgerOption);
            _dummyOptions.Add(_pizzaOption);

            _burgerVote = new Vote() { Id = 1, Poll = new Poll() { UUID = _mainUUID }, Option = new Option() { Id = 1 } };
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            _dummyVotes.Add(_burgerVote);

            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);
            _mainPoll = new Poll() { UUID = _mainUUID, Options = new List<Option>() { _burgerOption, _pizzaOption } };
            _emptyPoll = new Poll() { UUID = _emptyUUID, Options = new List<Option>() };
            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(_emptyPoll);

            _mainPoll.OptionAdding = false;
            _emptyPoll.OptionAdding = true;

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Options).Returns(_dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);

            _controller = new PollOptionController(mockContextFactory.Object);
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
            _controller.Get(_mainUUID);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetWithNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Get(newGuid);
        }

        [TestMethod]
        public void GetWithEmptyPollReturnsEmptyOptionList()
        {
            // Act
            var response = _controller.Get(_emptyUUID);

            // Assert
            Assert.AreEqual(0, response.Count);
        }

        [TestMethod]
        public void GetWithPopulatedPollReturnsOptionsForThatPoll()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            Assert.AreEqual(2, response.Count);
            CollectionAssert.AreEqual(new long[] { 1, 2 }, response.Select(r => r.Id).ToArray());
            CollectionAssert.AreEqual(new string[] { "Burger King", "Pizza Hut" }, response.Select(r => r.Name).ToArray());
        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.MethodNotAllowed)]
        public void PostIsNotAllowedOnPollsWithOptionTurnedOff()
        {
            // Act
            _controller.Post(_mainUUID, new OptionCreationRequestModel());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostWithNullOptionNotAllowed()
        {
            // Act
            _controller.Post(_emptyUUID, null);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PostForNonExistantPollNotFound()
        {
            // Act
            Guid invalidGuid = Guid.NewGuid();
            _controller.Post(invalidGuid, new OptionCreationRequestModel());

        }

        [TestMethod]
        public void PostAllowed()
        {
            // Act
            OptionCreationRequestModel newOption = new OptionCreationRequestModel() { Name = "Option" };
            _controller.Post(_emptyUUID, newOption);
        }

        #endregion
    }
}
