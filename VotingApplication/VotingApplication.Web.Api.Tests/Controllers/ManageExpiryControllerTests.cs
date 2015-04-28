using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Tests.TestHelpers;

namespace VotingApplication.Web.Tests.Controllers
{
    [TestClass]
    public class ManageExpiryControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("961efb70-6767-4658-a95d-fea312c802ec");

        [TestClass]
        public class PutTests : ManageExpiryControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollExpiryRequest request = new ManagePollExpiryRequest { };

                ManageExpiryController controller = CreateManageExpiryController(contextFactory);

                controller.Put(Guid.NewGuid(), request);
            }

            [TestMethod]
            public void NullExpiry_SetsExpiryToNull()
            {
                // Arrange
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollExpiryRequest request = new ManagePollExpiryRequest { };

                ManageExpiryController controller = CreateManageExpiryController(contextFactory);
                controller.Validate(request);

                // Act
                controller.Put(PollManageGuid, request);

                // Assert
                Assert.IsNull(existingPoll.ExpiryDate);
            }

            [TestMethod]
            public void SetExpiry_SetsExpiry()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                DateTime expiry = DateTime.Now.AddHours(1);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollExpiryRequest request = new ManagePollExpiryRequest { ExpiryDate = expiry };

                ManageExpiryController controller = CreateManageExpiryController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(expiry, existingPoll.ExpiryDate);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void PostExpiry_IsRejected()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                DateTime past = DateTime.Now.AddHours(-1);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollExpiryRequest request = new ManagePollExpiryRequest { ExpiryDate = past };

                ManageExpiryController controller = CreateManageExpiryController(contextFactory);

                controller.Put(PollManageGuid, request);
            }
        }

        public static ManageExpiryController CreateManageExpiryController(IContextFactory contextFactory)
        {
            return new ManageExpiryController(contextFactory)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        private Poll CreatePoll()
        {
            return new Poll()
            {
                ManageId = PollManageGuid
            };
        }
    }
}
