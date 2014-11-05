using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
    public class OptionSetControllerTests
    {
        private OptionSetController _controller;
        private OptionSet _colourOptionSet;
        private OptionSet _emptyOptionSet;
        private InMemoryDbSet<OptionSet> _dummyOptionSets;

        [TestInitialize]
        public void setup()
        {
            Option redOption = new Option() { Name = "Red" };
            Option blueOption = new Option() { Name = "Blue" };
            Option greenOption = new Option() { Name = "Green" };

            _colourOptionSet = new OptionSet() { Id = 1, Name = "Colours", Options = new List<Option>() { redOption, greenOption, blueOption } };
            _emptyOptionSet = new OptionSet() { Id = 2, Name = "Empty Set", Options = new List<Option>() };

            _dummyOptionSets = new InMemoryDbSet<OptionSet>(true);
            _dummyOptionSets.Add(_colourOptionSet);
            _dummyOptionSets.Add(_emptyOptionSet);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.OptionSets).Returns(_dummyOptionSets);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _controller = new OptionSetController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyOptionSets.Local.Count; i++)
            {
                _dummyOptionSets.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsAllOptionSets()
        {
            // Act
            var response = _controller.Get();

            // Assert
            List<OptionSet> expectedOptionSets = new List<OptionSet>() { _colourOptionSet, _emptyOptionSet };
            List<OptionSet> responseOptionSets = ((ObjectContent)response.Content).Value as List<OptionSet>;
            CollectionAssert.AreEquivalent(expectedOptionSets, responseOptionSets);
        }

        [TestMethod]
        public void GetByIdIsAllowed()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetByIdReturnsOptionSetWithMatchingId()
        {
            // Act
            var response = _controller.Get(2);

            // Assert
            OptionSet responseOptionSet = ((ObjectContent)response.Content).Value as OptionSet;
            Assert.AreEqual(_emptyOptionSet, responseOptionSet);
        }

        [TestMethod]
        public void GetNonexistentOptionSetByIdIsNotFound()
        {
            // Act
            var response = _controller.Get(99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("OptionSet 99 does not exist", error.Message);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(new OptionSet() { Name = "New OptionSet", Options = new List<Option>() });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostRejectsOptionSetWithoutAName()
        {
            // Act
            var response = _controller.Post(new OptionSet() { Options = new List<Option>() });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("OptionSet does not have a name", error.Message);
        }

        [TestMethod]
        public void PostRejectsOptionSetWithABlankName()
        {
            // Act
            var response = _controller.Post(new OptionSet() { Name = "", Options = new List<Option>() });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("OptionSet does not have a name", error.Message);
        }

        [TestMethod]
        public void PostAcceptsOptionSetWithoutAnOptionList()
        {
            // Act
            var response = _controller.Post(new OptionSet() { Name = "New OptionSet" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostOptionSetWithoutAnOptionListDefaultsToEmptyList()
        {
            // Act
            OptionSet newOptionSet = new OptionSet() { Name = "New OptionSet" };
            var response = _controller.Post(newOptionSet);

            // Assert
            CollectionAssert.AreEquivalent(new List<Option>(), newOptionSet.Options);
        }

        [TestMethod]
        public void PostOptionSetWithAnOptionListRetainsOptionList()
        {
            // Act
            List<Option> optionList = new List<Option>();
            Option purpleOption = new Option() { Name = "Purple" };
            optionList.Add(purpleOption);

            OptionSet newOptionSet = new OptionSet() { Name = "New OptionSet", Options = optionList };
            var response = _controller.Post(newOptionSet);

            // Assert
            CollectionAssert.AreEquivalent(optionList, newOptionSet.Options);
        }

        [TestMethod]
        public void PostWithValidOptionSetReturnsNewOptionSetId()
        {
            // Act
            OptionSet newOptionSet = new OptionSet() { Name = "New OptionSet" };
            var response = _controller.Post(newOptionSet);

            // Assert
            long responseId = (long)((ObjectContent)response.Content).Value;
            Assert.AreEqual(3, responseId);
        }

        [TestMethod]
        public void PostWithValidOptionSetAssignsNewOptionSetId()
        {
            // Act
            OptionSet newOptionSet = new OptionSet() { Name = "New OptionSet" };
            var response = _controller.Post(newOptionSet);

            // Assert
            Assert.AreEqual(3, newOptionSet.Id);
        }

        [TestMethod]
        public void PostWithValidOptionSetAddsToOptionSets()
        {
            // Act
            OptionSet newOptionSet = new OptionSet() { Name = "New OptionSet" };
            var response = _controller.Post(newOptionSet);

            // Assign
            List<OptionSet> expectedSets = new List<OptionSet>();
            expectedSets.Add(_colourOptionSet);
            expectedSets.Add(_emptyOptionSet);
            expectedSets.Add(newOptionSet);
            CollectionAssert.AreEquivalent(expectedSets, _dummyOptionSets.Local);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, new OptionSet());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new OptionSet());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new OptionSet());

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
    }
}
