using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;

namespace VotingApplication.Web.Api.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        private UserController _controller;

        [TestInitialize]
        public void setup()
        {
            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Users).Returns(new List<User>());

            _controller = new UserController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        [TestMethod]
        public void GetReturnsAllUsers()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
