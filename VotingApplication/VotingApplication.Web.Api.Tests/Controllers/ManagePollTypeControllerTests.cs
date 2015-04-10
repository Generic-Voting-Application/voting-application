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
    public class ManagePollTypeControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("961efb70-6767-4658-a95d-fea312c802ec");

        [TestClass]
        public class PutTests : ManagePollTypeControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManagePollTypeRequest request = new ManagePollTypeRequest { };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(Guid.NewGuid(), request);
            }

            [TestMethod]
            public void SetPollType_SetsPollType()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "UpDown", MaxPerVote = 1, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(PollType.UpDown, existingPoll.PollType);
            }

            [TestMethod]
            public void SetMaxValues_SetsMaxValues()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();

                int maxPerVote = 15;
                int maxPoints = 17;

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "UpDown", MaxPerVote = maxPerVote, MaxPoints = maxPoints };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(maxPerVote, existingPoll.MaxPerVote);
                Assert.AreEqual(maxPoints, existingPoll.MaxPoints);
            }

            [TestMethod]
            public void ChangedPollType_ClearsVotes()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPolls.Add(existingPoll);

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "UpDown", MaxPerVote = 1, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(0, existingVotes.Local.Count);
            }

            [TestMethod]
            public void UnChangedPollType_LeavesVotes()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                Poll existingPoll = CreatePoll();
                existingPoll.PollType = PollType.Basic;
                existingPolls.Add(existingPoll);

                Vote testVote = new Vote() { Poll = existingPoll };

                IDbSet<Ballot> existingBallots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                IDbSet<Vote> existingVotes = DbSetTestHelper.CreateMockDbSet<Vote>();
                existingVotes.Add(testVote);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls, existingBallots, existingVotes);
                ManagePollTypeRequest request = new ManagePollTypeRequest { PollType = "Basic", MaxPerVote = 1, MaxPoints = 1 };

                ManagePollTypeController controller = CreateManagePollTypeController(contextFactory);

                controller.Put(PollManageGuid, request);

                Assert.AreEqual(1, existingVotes.Local.Count);
            }
        }

        public static ManagePollTypeController CreateManagePollTypeController(IContextFactory contextFactory)
        {
            return new ManagePollTypeController(contextFactory)
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
