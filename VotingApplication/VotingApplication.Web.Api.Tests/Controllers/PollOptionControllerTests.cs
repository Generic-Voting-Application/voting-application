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

            _burgerVote = new Vote() { Id = 1, PollId = _mainUUID, OptionId = 1 };
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
        [ExpectedException(typeof(HttpResponseException))]
        public void GetWithNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            try
            {
                _controller.Get(newGuid);
            }
            catch (HttpResponseException e)
            {
                // Assert
                Assert.AreEqual(HttpStatusCode.NotFound, e.Response.StatusCode);
                throw;
            }
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
        [ExpectedException(typeof(HttpResponseException))]
        public void PostIsNotAllowedOnPollsWithOptionTurnedOff()
        {
            // Act
            try
            {
                _controller.Post(_mainUUID, new OptionCreationRequestModel());
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(HttpStatusCode.MethodNotAllowed, e.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void PostWithNullOptionNotAllowed()
        {
            // Act
            try
            {
                _controller.Post(_emptyUUID, null);
            }
            catch (HttpResponseException e)
            {
                // Assert
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void PostForNonExistantPollNotAllowed()
        {
            // Act
            Guid invalidGuid = Guid.NewGuid();
            try
            {
                _controller.Post(invalidGuid, new OptionCreationRequestModel());
            }
            catch (HttpResponseException e)
            {
                // Assert
                Assert.AreEqual(HttpStatusCode.NotFound, e.Response.StatusCode);
                throw;
            }

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
