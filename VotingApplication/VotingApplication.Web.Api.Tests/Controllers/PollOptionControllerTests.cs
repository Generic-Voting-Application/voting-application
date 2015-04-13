using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Tests.TestHelpers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class PollOptionControllerTests
    {
        [TestClass]
        public class PostTests
        {
            public readonly Guid PollManageGuid = new Guid("EBDE4ED9-D014-4145-B998-13B5A247BB4B");

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullRequest_ThrowsBadRequest()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollOptionController controller = CreatePollOptionController(contextFactory);


                controller.Post(PollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollOptionController controller = CreatePollOptionController(contextFactory);

                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel();

                controller.Post(PollManageGuid, optionCreationRequestModel);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.MethodNotAllowed)]
            public void OptionAddingNotAllowedForPoll_ThrowsMethodNotAllowed()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { UUID = PollManageGuid, OptionAdding = false });

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                PollOptionController controller = CreatePollOptionController(contextFactory);

                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel();

                controller.Post(PollManageGuid, optionCreationRequestModel);
            }

            [TestMethod]
            public void OptionAddingAllowedForPoll_AddsOption()
            {
                const string optionName = "new option";
                const string optionDescription = "description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();

                polls.Add(new Poll() { UUID = PollManageGuid, OptionAdding = true });


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);
                PollOptionController controller = CreatePollOptionController(contextFactory);


                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel()
                {
                    Name = optionName,
                    Description = optionDescription,
                };

                controller.Post(PollManageGuid, optionCreationRequestModel);

                Assert.AreEqual(1, options.Count());
                Assert.AreEqual(optionName, options.First().Name);
                Assert.AreEqual(optionDescription, options.First().Description);
            }

            [TestMethod]
            public void OptionAddingAllowedForPoll_AddsOptionToPoll()
            {
                const string optionName = "new option";
                const string optionDescription = "description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();

                Poll poll = new Poll()
                {
                    UUID = PollManageGuid,
                    OptionAdding = true
                };
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);
                PollOptionController controller = CreatePollOptionController(contextFactory);


                OptionCreationRequestModel optionCreationRequestModel = new OptionCreationRequestModel()
                {
                    Name = optionName,
                    Description = optionDescription,
                };

                controller.Post(PollManageGuid, optionCreationRequestModel);

                Assert.AreEqual(1, poll.Options.Count);
                Assert.AreEqual(optionName, poll.Options.First().Name);
                Assert.AreEqual(optionDescription, poll.Options.First().Description);
            }

            public static PollOptionController CreatePollOptionController(IContextFactory contextFactory)
            {
                return new PollOptionController(contextFactory)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }
        }
    }
}
