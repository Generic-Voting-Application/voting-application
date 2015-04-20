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
using VotingApplication.Web.Api.Tests.TestHelpers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class ManageMiscControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("961efb70-6767-4658-a95d-fea312c802ec");

        [TestClass]
        public class PutTests : ManageMiscControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { };

                ManageMiscController controller = CreateManageExpiryController(contextFactory);

                controller.Put(Guid.NewGuid(), request);
            }

            [TestMethod]
            public void SetMiscConfig_SetsMiscSettings()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                existingPoll.NamedVoting = false;
                existingPoll.InviteOnly = false;
                existingPoll.OptionAdding = false;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollMiscRequest request = new ManagePollMiscRequest { InviteOnly = true, NamedVoting = true, OptionAdding = true };

                ManageMiscController controller = CreateManageExpiryController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.IsTrue(existingPoll.InviteOnly);
                Assert.IsTrue(existingPoll.NamedVoting);
                Assert.IsTrue(existingPoll.OptionAdding);
            }
        }

        public static ManageMiscController CreateManageExpiryController(IContextFactory contextFactory)
        {
            return new ManageMiscController(contextFactory, null)
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
