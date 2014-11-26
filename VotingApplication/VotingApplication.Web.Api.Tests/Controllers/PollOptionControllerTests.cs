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
    public class PollOptionControllerTests
    {
        private PollOptionController _controller;
        private Guid _mainUUID;
        private Guid _emptyUUID;
        private Option _burgerOption;
        private Option _pizzaOption;
        private Vote _burgerVote;
        private Poll _mainPoll;
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
            Poll emptyPoll = new Poll() { UUID = _emptyUUID, Options = new List<Option>() };
            dummyPolls.Add(_mainPoll);
            dummyPolls.Add(emptyPoll);

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
            var response = _controller.Get(_mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithNonexistentSessionIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Session " + newGuid + " does not exist", error.Message);
        }

        [TestMethod]
        public void GetWithEmptySessionReturnsEmptyOptionList()
        {
            // Act
            var response = _controller.Get(_emptyUUID);
            
            // Assert
            List<Option> expectedOptions = new List<Option>();
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;
            CollectionAssert.AreEquivalent(expectedOptions, responseOptions);
        }

        [TestMethod]
        public void GetWithPopulatedSessionReturnsOptionsForThatSession()
        {
            // Act
            var response = _controller.Get(_mainUUID);
            
            // Assert
            List<Option> expectedOptions = new List<Option>() { _burgerOption, _pizzaOption };
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;
            CollectionAssert.AreEquivalent(expectedOptions, responseOptions);
        }

        [TestMethod]
        public void GetByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Get(_mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_mainUUID, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_mainUUID, 1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_mainUUID, 1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(_mainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(_mainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
