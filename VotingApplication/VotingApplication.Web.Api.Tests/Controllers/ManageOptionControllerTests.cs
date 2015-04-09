using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
using System.Linq;
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
    public class ManageOptionControllerTests
    {
        [TestClass]
        public class PutTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullUpdateRequest_ThrowsError()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");

                var controller = new ManageOptionController();

                controller.Put(pollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownPollManageGuid_ThrowsNotFound()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");
                var request = new ManageOptionUpdateRequest();

                var controller = new ManageOptionController();

                controller.Put(pollManageGuid, request);
            }

            [TestMethod]
            public void OptionWithNoPollOptionNumber_IsAddedToOptions()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { ManageId = pollManageGuid });

                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = null
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(pollManageGuid, request);

                Assert.AreEqual(1, options.Count());
                Assert.AreEqual(optionName, options.First().Name);
                Assert.AreEqual(optionDescription, options.First().Description);
            }

            [TestMethod]
            public void OptionWithNoPollOptionNumber_IsAddedToPoll()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();

                var poll = new Poll() { ManageId = pollManageGuid };
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = null
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(pollManageGuid, request);

                Assert.AreEqual(1, poll.Options.Count());
            }

            [TestMethod]
            public void OptionWithPollOptionNumber_IsUpdated()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;

                const string newOptionName = "Some other option";
                const string newOptionDescription = "Some other description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = pollManageGuid
                };


                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                poll.Options.Add(option);

                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = newOptionName,
                    Description = newOptionDescription,
                    PollOptionNumber = pollOptionNumber
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(pollManageGuid, request);


                Assert.AreEqual(newOptionName, options.First().Name);
                Assert.AreEqual(newOptionDescription, options.First().Description);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void OptionWithUnknownPollOptionNumber_ThrowsNotFound()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;
                const int unknownPollOptionNumber = 3;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = pollManageGuid
                };

                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                poll.Options.Add(option);

                options.Add(option);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();
                var optionRequest = new OptionUpdate()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = unknownPollOptionNumber
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(pollManageGuid, request);
            }

            [TestMethod]
            public void OptionWithPollOptionNumber_DoesNotAddNewOption()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;

                const string newOptionName = "Some other option";
                const string newOptionDescription = "Some other description";


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = pollManageGuid
                };


                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                poll.Options.Add(option);

                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, options);

                var request = new ManageOptionUpdateRequest();

                var optionRequest = new OptionUpdate()
                {
                    Name = newOptionName,
                    Description = newOptionDescription,
                    PollOptionNumber = pollOptionNumber
                };
                request.Options.Add(optionRequest);


                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(pollManageGuid, request);


                Assert.AreEqual(1, options.Count());
            }

            [TestMethod]
            public void OptionNotInRequest_RemovesOptionAndVotes()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");
                const string optionName = "Some Option";
                const string optionDescription = "Some Description";
                const int pollOptionNumber = 2;


                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = pollManageGuid
                };


                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option = new Option()
                {
                    Name = optionName,
                    Description = optionDescription,
                    PollOptionNumber = pollOptionNumber
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot();

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote();

                vote.Option = option;
                ballot.Votes.Add(vote);
                poll.Options.Add(option);

                votes.Add(vote);
                ballots.Add(ballot);
                options.Add(option);
                polls.Add(poll);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                var request = new ManageOptionUpdateRequest();

                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(pollManageGuid, request);


                Assert.AreEqual(0, poll.Options.Count());
                Assert.AreEqual(0, options.Count());
                Assert.AreEqual(0, votes.Count());
                Assert.AreEqual(0, ballot.Votes.Count());
            }

            [TestMethod]
            public void MultipleRequests()
            {
                var pollManageGuid = new Guid("433A8D8F-0FD7-4A44-94A7-10ECCD893400");

                const string optionRemoveName = "Some Option to remove";
                const string optionRemoveDescription = "Some Description to remove";
                const int optionRemoveNumber = 2;

                const string optionUpdateName = "Some Option to update";
                const string optionUpdateDescription = "Some Description to update";
                const int optionUpdateNumber = 1;

                const string updatedName = "An updated Name";
                const string updatedDescription = "An updated Description";

                const string addName = "A new option";
                const string addDescription = "A new description";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();

                var poll = new Poll()
                {
                    ManageId = pollManageGuid
                };
                polls.Add(poll);

                var optionToRemove = new Option()
                {
                    Name = optionRemoveName,
                    Description = optionRemoveDescription,
                    PollOptionNumber = optionRemoveNumber
                };
                var optionToUpdate = new Option()
                {
                    Name = optionUpdateName,
                    Description = optionUpdateDescription,
                    PollOptionNumber = optionUpdateNumber
                };
                options.Add(optionToRemove);
                options.Add(optionToUpdate);

                var ballot = new Ballot();
                ballots.Add(ballot);

                var vote = new Vote();
                votes.Add(vote);

                vote.Option = optionToRemove;
                ballot.Votes.Add(vote);
                poll.Options.Add(optionToRemove);
                poll.Options.Add(optionToUpdate);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                var request = new ManageOptionUpdateRequest();
                var updateOption = new OptionUpdate()
                {
                    Name = updatedName,
                    Description = updatedDescription,
                    PollOptionNumber = optionUpdateNumber
                };
                var addOption = new OptionUpdate()
                {
                    Name = addName,
                    Description = addDescription,
                    PollOptionNumber = null
                };
                request.Options.Add(updateOption);
                request.Options.Add(addOption);

                ManageOptionController controller = CreateManageOptionController(contextFactory);


                controller.Put(pollManageGuid, request);


                Assert.AreEqual(2, poll.Options.Count());
                Assert.AreEqual(2, options.Count());

                Assert.AreEqual(addName, options.First().Name);
                Assert.AreEqual(addDescription, options.First().Description);

                Assert.AreEqual(updatedName, options.Last().Name);
                Assert.AreEqual(updatedDescription, options.Last().Description);
                Assert.AreEqual(optionUpdateNumber, options.Last().PollOptionNumber);
            }
        }

        public static ManageOptionController CreateManageOptionController(IContextFactory contextFactory)
        {
            return new ManageOptionController(contextFactory)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }
    }
}
