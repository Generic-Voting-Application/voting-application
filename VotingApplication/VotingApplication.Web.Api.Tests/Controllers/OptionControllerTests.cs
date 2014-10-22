using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;

namespace VotingApplication.Web.Api.Tests
{
    [TestClass]
    public class OptionControllerTests
    {
        private OptionController _controller;
        private Option _burgerOption;
        private Option _pizzaOption;

        [TestInitialize]
        public void setup()
        {
            _burgerOption = new Option { Id = 1, Name = "Burger King" };
            _pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            dummyOptions.Add(_burgerOption);
            dummyOptions.Add(_pizzaOption);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);

            _controller = new OptionController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        [TestMethod]
        public void GetReturnsAllOptions()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsNonNullOptions()
        {
            // Act
            var response = _controller.Get();
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;

            // Assert
            Assert.IsNotNull(responseOptions);
        }

        [TestMethod]
        public void GetReturnsOptionsFromTheDatabase()
        {
            // Act
            var response = _controller.Get();
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_burgerOption);
            expectedOptions.Add(_pizzaOption);
            CollectionAssert.AreEquivalent(expectedOptions, responseOptions);
        }

        [TestMethod]
        public void GetWithIdFindsOptionWithId()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsOptionWithIdFromTheDatabase()
        {
            // Act
            var response = _controller.Get(2);
            Option responseOption = ((ObjectContent)response.Content).Value as Option;

            // Assert
            Assert.AreEqual(_pizzaOption.Id, responseOption.Id);
            Assert.AreEqual(_pizzaOption.Name, responseOption.Name);
        }

        [TestMethod]
        public void GetWithIdReturnsErrorCode404ForUnknownOptionID()
        {
            // Act
            var response = _controller.Get(3);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsNullUserForUnknownOptionID()
        {
            // Act
            var response = _controller.Get(3);
            Option responseOption = ((ObjectContent)response.Content).Value as Option;

            // Assert
            Assert.IsNull(responseOption);
        }
    }
}
