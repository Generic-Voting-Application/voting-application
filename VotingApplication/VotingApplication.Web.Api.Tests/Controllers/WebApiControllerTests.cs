using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Metrics;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class WebApiControllerTests

    {
        private WebApiController _controller;
        private InMemoryDbSet<Poll> _dummyPolls;
        private IVotingContext _mockContext;

        [TestInitialize]
        public void Setup()
        {
            _dummyPolls = new InMemoryDbSet<Poll>(true);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            _mockContext = mockContext.Object;
            mockContextFactory.Setup(a => a.CreateContext()).Returns(_mockContext);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);

            var mockMetricHandler = new Mock<IMetricEventHandler>();

            _controller = new WebApiController(mockContextFactory.Object, mockMetricHandler.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region Poll Retrieval

        [TestMethod]
        public void CanRetrievePollByPollId()
        {
            // Arrange
            Guid pollId = Guid.NewGuid();
            Poll createdPoll = new Poll { UUID = pollId };
            _dummyPolls.Add(createdPoll);

            // Act
            Poll retrievedPoll = _controller.PollByPollId(pollId, _mockContext);

            // Assert
            Assert.AreEqual(createdPoll, retrievedPoll);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void RetrievalOfMissingPollByPollIdIsNotFoundException()
        {
            // Act
            _controller.PollByPollId(Guid.NewGuid(), _mockContext);
        }

        [TestMethod]
        public void CanRetrievePollByManageId()
        {
            // Arrange
            Guid manageId = Guid.NewGuid();
            Poll createdPoll = new Poll { ManageId = manageId };
            _dummyPolls.Add(createdPoll);

            // Act
            Poll retrievedPoll = _controller.PollByManageId(manageId, _mockContext);

            // Assert
            Assert.AreEqual(createdPoll, retrievedPoll);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void RetrievalOfMissingPollByManageIdIsNotFoundException()
        {
            // Act
            _controller.PollByManageId(Guid.NewGuid(), _mockContext);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        // NB: Tries to force a Stack overflow
        public void RetrievalOfMissingPollWithRouteDataIsNotFoundException()
        {
            // Arrange
            IDictionary<string, object> data = new Dictionary<string, object>();
            var httpRoute = new HttpRoute("", new HttpRouteValueDictionary(data));
            var routeData = new HttpRouteData(httpRoute);
            routeData.Values.Add("manageId", Guid.NewGuid().ToString());
            routeData.Values.Add("pollId", null);

            var mockRequestContext = new Mock<HttpRequestContext>();
            mockRequestContext.Setup(a => a.RouteData).Returns(routeData);

            _controller.RequestContext = mockRequestContext.Object;

            // Act
            _controller.PollByManageId(Guid.NewGuid(), _mockContext);
        }

        #endregion
    }
}
