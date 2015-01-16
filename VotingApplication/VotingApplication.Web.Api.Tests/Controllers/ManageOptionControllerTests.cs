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
    public class ManageOptionControllerTests
    {
        private ManageOptionController _controller;
        private Guid _manageMainUUID;
        private Guid _manageEmptyUUID;
        private Option _burgerOption;
        private Option _pizzaOption;
        private Vote _burgerVote;
        private Poll _mainPoll;
        private InMemoryDbSet<Option> _dummyOptions;
        private InMemoryDbSet<Vote> _dummyVotes;
        InMemoryDbSet<Poll> _dummyPolls;

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
            _mainPoll = new Poll() { UUID = mainUUID, ManageId = _manageMainUUID, Options = new List<Option>() { _burgerOption, _pizzaOption } };
            Poll emptyPoll = new Poll() { UUID = emptyUUID, ManageId = _manageEmptyUUID, Options = new List<Option>() };
            _dummyPolls.Add(_mainPoll);
            _dummyPolls.Add(emptyPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Options).Returns(_dummyOptions);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);

            _controller = new ManageOptionController(mockContextFactory.Object);
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
            var response = _controller.Get(_manageMainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Get(newGuid);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void GetWithEmptyPollReturnsEmptyOptionList()
        {
            // Act
            var response = _controller.Get(_manageEmptyUUID);

            // Assert
            List<OptionRequestResponseModel> responseOptions = ((ObjectContent)response.Content).Value as List<OptionRequestResponseModel>;
            Assert.AreEqual(0, responseOptions.Count);
        }

        [TestMethod]
        public void GetWithPopulatedPollReturnsOptionsForThatPoll()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            List<OptionRequestResponseModel> responseOptions = ((ObjectContent)response.Content).Value as List<OptionRequestResponseModel>;
            Assert.AreEqual(2, responseOptions.Count);
            CollectionAssert.AreEqual(new string[] { "Burger King", "Pizza Hut" }, responseOptions.Select(r => r.Name).ToArray());
        }
        
        [TestMethod]
        public void GetByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Get(_manageMainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(_manageMainUUID, 1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "Abc", Description = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostInvalidInputIsRejected()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "");

            // Act
            var response = _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "", Description = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [TestMethod]
        public void PostWithoutDescriptionIsAccepted()
        {
            // Act
            var response = _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostWithoutMoreInfoIsAccepted()
        {
            // Act
            var response = _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "Abc", Description = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostAddsOptionToOptionList()
        {
            // Act
            OptionCreationRequestModel newOption = new OptionCreationRequestModel() { Name = "Bella Vista" };
            _controller.Post(_manageMainUUID, newOption);

            // Assert
            Assert.AreEqual(3, _mainPoll.Options.Count);
        }

        [TestMethod]
        public void PostReturnsNotFoundForMissingPoll()
        {
            // Act
            OptionCreationRequestModel newOption = new OptionCreationRequestModel() { Name = "Bella Vista" };
            var response = _controller.Post(Guid.NewGuid(), newOption);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void PostAddsOptionToPoll()
        {
            // Act
            OptionCreationRequestModel newOption = new OptionCreationRequestModel() { Name = "Bella Vista" };
            _controller.Post(_manageMainUUID, newOption);

            // Assert
            Assert.AreEqual(3, _mainPoll.Options.Count);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(_manageMainUUID, 1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsAllowed()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdRemovesOptionWithMatchingIdFromPoll()
        {
            // Act
            _controller.Delete(_manageMainUUID, 1);

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_pizzaOption);
            CollectionAssert.AreEquivalent(expectedOptions, _mainPoll.Options);
        }

        [TestMethod]
        public void DeleteByIdDoesNotRemoveOptionFromGlobalOptions()
        {
            // Act
            _controller.Delete(_manageMainUUID, 1);

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_burgerOption);
            expectedOptions.Add(_pizzaOption);
            CollectionAssert.AreEquivalent(expectedOptions, _dummyOptions.Local);
        }

        [TestMethod]
        public void DeleteByIdIsAllowedIfNoOptionMatchesId()
        {
            // Act
            var response = _controller.Delete(_manageMainUUID, 99);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsIsNotAllowedForMissingPollUUID()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            var response = _controller.Delete(newGuid, 1);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Poll " + newGuid + " not found", error.Message);
        }

        [TestMethod]
        public void DeleteOptionRemovesRelevantVotes()
        {
            // Act
            _controller.Delete(_manageMainUUID, 1);

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>(), _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteOptionDoesNotRemoveOtherVotes()
        {
            // Act
            _controller.Delete(_manageMainUUID, 2);

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>() { _burgerVote }, _dummyVotes.Local);
        }

        [TestMethod]
        public void DeleteOptionOnlyDeletesFromCurrentPoll()
        {
            // Act
            _controller.Delete(_manageEmptyUUID, 1);

            // Assert
            CollectionAssert.AreEquivalent(new List<Vote>() { _burgerVote }, _dummyVotes.Local);
        }


        #endregion

    }
}
