using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
    public class ManageVoterControllerTests
    {
        public readonly Guid PollManageGuid = new Guid("EBDE4ED9-D014-4145-B998-13B5A247BB4B");

        [TestClass]
        public class GetTests : ManageVoterControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void UnknownManageId_ReturnsNotFound()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);

                ManageVoterController controller = CreateManageVoteController(contextFactory);

                controller.Get(PollManageGuid);
            }

            [TestMethod]
            public void PollWithNoBallots_ReturnsEmptyList()
            {
                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();

                var poll = new Poll()
                 {
                     ManageId = PollManageGuid
                 };

                existingPolls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);

                ManageVoterController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(PollManageGuid);


                CollectionAssert.AreEqual(new List<ManageVoteResponseModel>(), response);
            }

            [TestMethod]
            public void PollWithBallotContainingNoVotes_ReturnsEmptyList()
            {
                Guid pollManageGuid = new Guid("EBDE4ED9-D014-4145-B998-13B5A247BB4B");

                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll =
                    new Poll()
                    {
                        ManageId = pollManageGuid,
                        Ballots = new List<Ballot>()
                        {
                            new Ballot()
                        }
                    };

                existingPolls.Add(poll);
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);

                ManageVoterController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(pollManageGuid);


                CollectionAssert.AreEqual(new List<ManageVoteResponseModel>(), response);
            }

            [TestMethod]
            public void PollWithBallotsWithVotes_ReturnsAll()
            {
                var manageGuid = new Guid("A76287F6-BC56-421C-9294-A477D1E9C4B3");
                const string voterName = "Derek";
                const string optionName = "Value?";
                const int optionValue = 23;

                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                var ballot = new Ballot()
                {
                    ManageGuid = manageGuid,
                    VoterName = voterName,
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var vote = new Vote()
                {
                    Option = new Option()
                    {
                        Name = optionName
                    },
                    VoteValue = optionValue
                };


                ballot.Votes.Add(vote);
                poll.Ballots.Add(ballot);
                existingPolls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(PollManageGuid);


                Assert.AreEqual(1, response.Count);

                ManageVoteResponseModel responseBallot = response[0];

                Assert.AreEqual(manageGuid, responseBallot.BallotManageGuid);
                Assert.AreEqual(voterName, responseBallot.VoterName);
                Assert.AreEqual(1, responseBallot.Votes.Count);

                VoteResponse responseVote = responseBallot.Votes[0];

                Assert.AreEqual(optionName, responseVote.OptionName);
                Assert.AreEqual(optionValue, responseVote.Value);
            }

            [TestMethod]
            public void MultiplePolls_ReturnsOnlyRequested()
            {
                Guid otherManageGuid = new Guid("35FC553F-2F0E-49E0-A919-802900262046");

                const string expectedVoterName = "Expected Voter";
                const string otherVoterName = "Someone else voting";


                const string expectedOptionName = "SomeOption";
                var expectedOption = new Option() { Name = expectedOptionName };
                const int expectedVoteValue = 42;

                var otherOption = new Option() { Name = "Some Other Option" };
                const int otherVoteValue = 16;


                IDbSet<Poll> existingPolls = DbSetTestHelper.CreateMockDbSet<Poll>();

                var poll1 = new Poll()
                {
                    ManageId = PollManageGuid,
                    Ballots = new List<Ballot>()
                    {
                        CreateBallot(expectedVoterName, CreateVote(expectedOption, expectedVoteValue))
                    }
                };
                var poll2 =
                    new Poll()
                    {
                        ManageId = otherManageGuid,
                        Ballots = new List<Ballot>()
                        {
                            CreateBallot(otherVoterName, CreateVote(otherOption, otherVoteValue))
                        }
                    };
                existingPolls.Add(poll1);
                existingPolls.Add(poll2);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(existingPolls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                List<ManageVoteResponseModel> response = controller.Get(PollManageGuid);

                Assert.AreEqual(1, response.Count);

                ManageVoteResponseModel responseBallot = response[0];

                Assert.AreEqual(expectedVoterName, responseBallot.VoterName);
                Assert.AreEqual(1, responseBallot.Votes.Count);

                VoteResponse responseVote = responseBallot.Votes[0];

                Assert.AreEqual(expectedOptionName, responseVote.OptionName);
                Assert.AreEqual(42, responseVote.Value);
            }
        }

        [TestClass]
        public class DeleteAllTests : ManageVoterControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void NoMatchingPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);
            }

            [TestMethod]
            public void BallotsAreRemovedFromPoll()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot()
                {
                    VoterName = "Derek",
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var ballot2 = new Ballot()
                {
                    VoterName = "Mavis",
                    TokenGuid = new Guid("C4865232-2A1A-4BB5-8EF7-5F6C17CFAC8A")
                };


                poll.Ballots.Add(ballot1);
                poll.Ballots.Add(ballot2);

                ballots.Add(ballot1);
                ballots.Add(ballot2);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);

                Assert.AreEqual(0, ballots.Count());
            }

            [TestMethod]
            public void VotesAreRemovedFromBallots()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot()
                {
                    VoterName = "Derek",
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var ballot2 = new Ballot()
                {
                    VoterName = "Mavis",
                    TokenGuid = new Guid("C4865232-2A1A-4BB5-8EF7-5F6C17CFAC8A")
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote();
                var vote2 = new Vote();
                var vote3 = new Vote();
                var vote4 = new Vote();



                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);
                ballot2.Votes.Add(vote3);
                ballot2.Votes.Add(vote4);

                poll.Ballots.Add(ballot1);
                poll.Ballots.Add(ballot2);

                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);
                votes.Add(vote4);

                ballots.Add(ballot1);
                ballots.Add(ballot2);

                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);

                Assert.AreEqual(0, ballots.Count());
                Assert.AreEqual(0, votes.Count());
            }

            [TestMethod]
            public void MultiplePolls_OnlyAffectsRequested()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll1 = new Poll()
                {
                    ManageId = PollManageGuid
                };
                var poll2 = new Poll()
                {
                    ManageId = new Guid("600D6D95-2C77-483E-B708-5946B848161D")
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot()
                {
                    VoterName = "Derek",
                    TokenGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1")
                };

                var ballot2 = new Ballot()
                {
                    VoterName = "Mavis",
                    TokenGuid = new Guid("C4865232-2A1A-4BB5-8EF7-5F6C17CFAC8A")
                };

                var ballot3 = new Ballot()
                {
                    VoterName = "Boris",
                    TokenGuid = new Guid("606E358F-B1BE-445A-AE93-2E5F5A824D67")
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Id = 1 };
                var vote2 = new Vote() { Id = 2 };
                var vote3 = new Vote() { Id = 3 };
                var vote4 = new Vote() { Id = 4 };
                var vote5 = new Vote() { Id = 5 };
                var vote6 = new Vote() { Id = 6 };



                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);
                ballot2.Votes.Add(vote3);
                ballot2.Votes.Add(vote4);
                ballot3.Votes.Add(vote5);
                ballot3.Votes.Add(vote6);

                poll1.Ballots.Add(ballot1);
                poll1.Ballots.Add(ballot2);

                poll2.Ballots.Add(ballot3);

                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);
                votes.Add(vote4);
                votes.Add(vote5);
                votes.Add(vote6);

                ballots.Add(ballot1);
                ballots.Add(ballot2);
                ballots.Add(ballot3);

                polls.Add(poll1);
                polls.Add(poll2);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid);

                Assert.AreEqual(1, ballots.Count());
                Assert.AreEqual(ballot3, ballots.Single());

                Assert.AreEqual(2, votes.Count());
                Assert.AreEqual(5, votes.First().Id);
                Assert.AreEqual(6, votes.Last().Id);
            }
        }

        [TestClass]
        public class DeleteBallotTests : ManageVoterControllerTests
        {
            public readonly Guid ManageBallotGuid = new Guid("F4819B59-46A1-47D3-BD23-DB791032A099");

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void NoMatchingPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void MatchingPoll_NoMatchingBallot_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { UUID = PollManageGuid });

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void BallotDoesNotBelongToPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { ManageId = PollManageGuid });

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                ballots.Add(new Ballot() { ManageGuid = ManageBallotGuid });


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);
            }

            [TestMethod]
            public void BallotIsRemoved()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = ManageBallotGuid,
                    VoterName = "Derek"
                };


                poll.Ballots.Add(ballot);

                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);

                Assert.AreEqual(0, ballots.Count());
            }

            [TestMethod]
            public void BallotIsRemovedFromPoll()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = ManageBallotGuid,
                    VoterName = "Derek"
                };


                poll.Ballots.Add(ballot);

                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);

                Assert.AreEqual(0, poll.Ballots.Count());
            }

            [TestMethod]
            public void OtherBallotsAreNotRemoved()
            {
                var otherManageGuid = new Guid("7C9FCB6E-ACC2-4170-8A3E-AE18D6AA9061");

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot()
                {
                    ManageGuid = ManageBallotGuid,
                    VoterName = "Derek"
                };

                var ballot2 = new Ballot()
                {
                    ManageGuid = otherManageGuid,
                    VoterName = "Mavis"
                };


                poll.Ballots.Add(ballot1);
                poll.Ballots.Add(ballot2);

                ballots.Add(ballot1);
                ballots.Add(ballot2);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);

                Assert.AreEqual(1, ballots.Count());
                Assert.AreEqual(otherManageGuid, ballots.Single().ManageGuid);
            }

            [TestMethod]
            public void VotesAreRemoved()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = ManageBallotGuid,
                    VoterName = "Derek"
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote();

                ballot.Votes.Add(vote);
                poll.Ballots.Add(ballot);

                votes.Add(vote);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);


                Assert.AreEqual(0, votes.Count());
            }

            [TestMethod]
            public void VotesAreRemovedFromBallot()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = ManageBallotGuid,
                    VoterName = "Derek"
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote();

                ballot.Votes.Add(vote);
                poll.Ballots.Add(ballot);

                votes.Add(vote);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);


                Assert.AreEqual(0, ballot.Votes.Count);
            }

            [TestMethod]
            public void OtherVotesAreNotRemoved()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    ManageId = PollManageGuid
                };

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = ManageBallotGuid,
                    VoterName = "Derek"
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote();
                var vote2 = new Vote();

                ballot.Votes.Add(vote1);
                poll.Ballots.Add(ballot);

                votes.Add(vote1);
                votes.Add(vote2);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, ManageBallotGuid);


                Assert.AreEqual(1, votes.Count());
            }
        }

        public static ManageVoterController CreateManageVoteController(IContextFactory contextFactory)
        {
            return new ManageVoterController(contextFactory)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        public Vote CreateVote(Option option, int value)
        {
            return new Vote() { Option = option, VoteValue = value };
        }

        public Ballot CreateBallot(string voterName, Vote vote)
        {
            return new Ballot() { VoterName = voterName, Votes = new List<Vote>() { vote } };
        }
    }
}
