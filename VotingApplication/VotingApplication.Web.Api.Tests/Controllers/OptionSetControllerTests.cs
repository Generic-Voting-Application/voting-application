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

        [TestInitialize]
        public void setup()
        {
            Option redOption = new Option() { Name = "Red" };
            Option blueOption = new Option() { Name = "Blue" };
            Option greenOption = new Option() { Name = "Green" };

            _colourOptionSet = new OptionSet() { Id = 1, Name = "Colours", Options = new List<Option>() { redOption, greenOption, blueOption } };
            _emptyOptionSet = new OptionSet() { Id = 2, Name = "Empty Set", Options = new List<Option>() };

            InMemoryDbSet<OptionSet> dummyOptionSets = new InMemoryDbSet<OptionSet>(true);
            dummyOptionSets.Add(_colourOptionSet);
            dummyOptionSets.Add(_emptyOptionSet);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.OptionSets).Returns(dummyOptionSets);

            _controller = new OptionSetController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
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
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post(new OptionSet());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
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
