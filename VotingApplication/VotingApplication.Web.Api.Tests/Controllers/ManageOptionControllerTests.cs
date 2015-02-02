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
            Assert.AreEqual(0, response.Count);
        }

        [TestMethod]
        public void GetWithPopulatedPollReturnsOptionsForThatPoll()
        {
            // Act
            var response = _controller.Get(_manageMainUUID);

            // Assert
            Assert.AreEqual(2, response.Count);
            CollectionAssert.AreEqual(new string[] { "Burger King", "Pizza Hut" }, response.Select(r => r.Name).ToArray());
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsAllowed()
        {
            // Act
            _controller.Put(_manageMainUUID, new List<Option>());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PutReturnsNotFoundForMissingPoll()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Put(newGuid, new List<Option>() { });
        }

        [TestMethod]
        public void PutOverwritesOptionsOnAPoll()
        {
            // Act
            Option newOption = new Option() { Id = 101, Name = "Put Option" };
            List<Option> options = new List<Option>() { newOption };
            _controller.Put(_manageMainUUID, options);

            // Assert
            CollectionAssert.AreEquivalent(_mainPoll.Options, options);
            Assert.AreEqual(0, _mainPoll.Options[0].Id);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutWithoutNameIsRejected()
        {
            // Act
            Option newOption = new Option() { Description = "Abc", Info = "Abc" };
            _controller.Put(_manageMainUUID, new List<Option> { newOption });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PutWithEmptyNameIsRejected()
        {
            // Act
            Option newOption = new Option() { Name = "", Description = "Abc", Info = "Abc" };
            _controller.Put(_manageMainUUID, new List<Option> { newOption });
        }

        [TestMethod]
        public void PutWithoutDescriptionIsAccepted()
        {
            // Act
            Option newOption = new Option() { Name = "Abc", Info = "Abc" };
            _controller.Put(_manageMainUUID, new List<Option> { newOption });
        }

        [TestMethod]
        public void PutWithoutMoreInfoIsAccepted()
        {
            // Act
            Option newOption = new Option() { Name = "Abc", Description = "Abc" };
            _controller.Put(_manageMainUUID, new List<Option> { newOption });
        }

        [TestMethod]
        public void PutReturnsUpdatedOptionList()
        {
            // Act
            Option newOption = new Option() { Id = 101, Name = "Put Option" };
            List<Option> options = new List<Option>() { newOption };
            var response = _controller.Put(_manageMainUUID, options);

            // Assert
            CollectionAssert.AreEquivalent(response, options);
            Assert.AreEqual(0, response[0].Id);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "Abc", Description = "Abc", Info = "Abc" });

        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostInvalidInputIsRejected()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "");

            // Act
            _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "", Description = "Abc", Info = "Abc" });
        }

        [TestMethod]
        public void PostWithoutDescriptionIsAccepted()
        {
            // Act
            _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "Abc", Info = "Abc" });
        }

        [TestMethod]
        public void PostWithoutMoreInfoIsAccepted()
        {
            // Act
            _controller.Post(_manageMainUUID, new OptionCreationRequestModel() { Name = "Abc", Description = "Abc" });
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
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void PostReturnsNotFoundForMissingPoll()
        {
            // Act
            OptionCreationRequestModel newOption = new OptionCreationRequestModel() { Name = "Bella Vista" };
            _controller.Post(Guid.NewGuid(), newOption);
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
        #endregion

        #region DELETE

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
            _controller.Delete(_manageMainUUID, 99);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void DeleteByIdIsIsNotAllowedForMissingPollUUID()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Delete(newGuid, 1);
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
