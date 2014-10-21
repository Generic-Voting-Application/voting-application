using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
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

        [TestInitialize]
        public void setup()
        {
            _burgerOption = new Option { Id = 1, Name = "Burger King" };
            List<Option> dummyOptions = new List<Option>();
            dummyOptions.Add(_burgerOption);

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
            CollectionAssert.AreEquivalent(expectedOptions, responseOptions);
        }
    }
}
