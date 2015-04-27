using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
using VotingApplication.Web.Api.Metrics;
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

                const int optionNumber = 1;
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
                        PollOptionNumber = optionNumber,
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
                Assert.AreEqual(optionNumber, responseVote.OptionNumber);
            }

            [TestMethod]
            public void MultiplePolls_ReturnsOnlyRequested()
            {
                Guid otherManageGuid = new Guid("35FC553F-2F0E-49E0-A919-802900262046");

                const string expectedVoterName = "Expected Voter";
                const string otherVoterName = "Someone else voting";



                const string expectedOptionName = "SomeOption";
                const int expectedOptionNumber = 3;

                var expectedOption = new Option() { PollOptionNumber = expectedOptionNumber, Name = expectedOptionName };
                const int expectedVoteValue = 42;

                const int otherOptionNumber = 1;
                var otherOption = new Option() { PollOptionNumber = otherOptionNumber, Name = "Some Other Option" };
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
                Assert.AreEqual(expectedVoteValue, responseVote.Value);
                Assert.AreEqual(expectedOptionNumber, responseVote.OptionNumber);
            }
        }

        [TestClass]
        public class DeleteAllTests : ManageVoterControllerTests
        {
            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NullRequest_ThrowsBadRequest()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                controller.Delete(PollManageGuid, null);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NoBallotRequestsInRequest_ThrowsBadRequest()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var request = new DeleteVotersRequestModel();


                controller.Delete(PollManageGuid, request);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NoVoteRequestsInRequest_ThrowsBadRequest()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var request = new DeleteVotersRequestModel();
                request.BallotDeleteRequests.Add(new DeleteBallotRequestModel());


                controller.Delete(PollManageGuid, request);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void NoMatchingPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var deleteBallotRequestModel = new DeleteBallotRequestModel();
                deleteBallotRequestModel.VoteDeleteRequests.Add(new DeleteVoteRequestModel());

                var request = new DeleteVotersRequestModel();
                request.BallotDeleteRequests.Add(deleteBallotRequestModel);

                controller.Delete(PollManageGuid, request);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void RequestedBallots_DoNotBelongToPoll()
            {
                var ballotManageGuid = new Guid("1AC3FABB-A077-4EF3-84DC-62074BA8FDF1");

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreatePoll();

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = ballotManageGuid
                };

                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                ManageVoterController controller = CreateManageVoteController(contextFactory);


                var request = new DeleteVotersRequestModel();
                var ballotDeleteRequest = new DeleteBallotRequestModel() { BallotManageGuid = ballotManageGuid };
                DeleteVoteRequestModel voteRequest = new DeleteVoteRequestModel();

                ballotDeleteRequest.VoteDeleteRequests.Add(voteRequest);
                request.BallotDeleteRequests.Add(ballotDeleteRequest);


                controller.Delete(PollManageGuid, request);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void RequestedVotes_DoNotBelongToBallot()
            {
                const int pollOptionNumber1 = 1;
                const int pollOptionNumber2 = 2;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreatePoll();

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = PollManageGuid
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote()
                {
                    Option = new Option()
                    {
                        PollOptionNumber = pollOptionNumber1
                    }
                };



                ballot.Votes.Add(vote);
                poll.Ballots.Add(ballot);

                votes.Add(vote);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var voteRequest = new DeleteVoteRequestModel { OptionNumber = pollOptionNumber2 };
                var ballotRequest = new DeleteBallotRequestModel { BallotManageGuid = PollManageGuid };
                ballotRequest.VoteDeleteRequests.Add(voteRequest);

                var request = new DeleteVotersRequestModel();
                request.BallotDeleteRequests.Add(ballotRequest);


                controller.Delete(PollManageGuid, request);
            }

            [TestMethod]
            public void RequestedVotesAreRemoved()
            {
                const int pollOptionNumber = 1;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreatePoll();

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = PollManageGuid
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote()
                {
                    Option = new Option()
                    {
                        PollOptionNumber = pollOptionNumber
                    }
                };



                ballot.Votes.Add(vote);
                poll.Ballots.Add(ballot);

                votes.Add(vote);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var voteRequest = new DeleteVoteRequestModel { OptionNumber = pollOptionNumber };
                var ballotRequest = new DeleteBallotRequestModel { BallotManageGuid = PollManageGuid };
                ballotRequest.VoteDeleteRequests.Add(voteRequest);

                var request = new DeleteVotersRequestModel();
                request.BallotDeleteRequests.Add(ballotRequest);


                controller.Delete(PollManageGuid, request);


                Assert.AreEqual(0, votes.Count());
            }

            [TestMethod]
            public void ClearedVotesGenerateVoteDeletionMetric()
            {
                // Arrange
                Option option = new Option() { PollOptionNumber = 1, Id = 1, Name = "Option" };
                Poll poll = new Poll() { UUID = Guid.NewGuid(), ManageId = Guid.NewGuid(), Options = new List<Option>() { option } };
                Ballot ballot = new Ballot() { TokenGuid = Guid.NewGuid(), Votes = new List<Vote>(), ManageGuid = Guid.NewGuid() };
                Vote voteToClear = new Vote() { Ballot = ballot, Poll = poll, VoteValue = 1, Option = option };
                
                ballot.Votes.Add(voteToClear);
                poll.Ballots.Add(ballot);

                var options = DbSetTestHelper.CreateMockDbSet<Option>();
                var polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var votes = DbSetTestHelper.CreateMockDbSet<Vote>();

                options.Add(option);
                polls.Add(poll);
                ballots.Add(ballot);
                votes.Add(voteToClear);

                var metricHandler = new Mock<IMetricHandler>();
                var contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);
                ManageVoterController controller = CreateManageVoteController(contextFactory, metricHandler.Object);

                // Act
                DeleteVoteRequestModel voteDelete = new DeleteVoteRequestModel() { OptionNumber = 1 };
                DeleteBallotRequestModel deletion = new DeleteBallotRequestModel() { BallotManageGuid = ballot.ManageGuid, VoteDeleteRequests = new List<DeleteVoteRequestModel>() { voteDelete } };
                List<DeleteBallotRequestModel> ballotDeletions = new List<DeleteBallotRequestModel>() { deletion };
                DeleteVotersRequestModel request = new DeleteVotersRequestModel() { BallotDeleteRequests = ballotDeletions };
                controller.Delete(poll.ManageId, request);

                // Assert
                metricHandler.Verify(m => m.VoteDeletedEvent(voteToClear, poll.UUID), Times.Once());
            }

            [TestMethod]
            public void AllVotesRequestedRemovalForBallot_RemovesBallot()
            {
                const int pollOptionNumber = 1;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreatePoll();

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = PollManageGuid
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote()
                {
                    Option = new Option()
                    {
                        PollOptionNumber = pollOptionNumber
                    }
                };



                ballot.Votes.Add(vote);
                poll.Ballots.Add(ballot);

                votes.Add(vote);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var voteRequest = new DeleteVoteRequestModel { OptionNumber = pollOptionNumber };
                var ballotRequest = new DeleteBallotRequestModel { BallotManageGuid = PollManageGuid };
                ballotRequest.VoteDeleteRequests.Add(voteRequest);

                var request = new DeleteVotersRequestModel();
                request.BallotDeleteRequests.Add(ballotRequest);


                controller.Delete(PollManageGuid, request);


                Assert.AreEqual(0, ballots.Count());
                Assert.AreEqual(0, poll.Ballots.Count());
            }

            [TestMethod]
            public void SomeVotesRequestedRemovalForBallot_DoesNotRemovesBallot()
            {
                const int pollOptionNumber1 = 1;
                const int pollOptionNumber2 = 5;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreatePoll();

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = PollManageGuid
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote()
                {
                    Option = new Option()
                    {
                        PollOptionNumber = pollOptionNumber1
                    }
                };
                var vote2 = new Vote()
                {
                    Option = new Option()
                    {
                        PollOptionNumber = pollOptionNumber2
                    }
                };


                ballot.Votes.Add(vote1);
                ballot.Votes.Add(vote2);
                poll.Ballots.Add(ballot);

                votes.Add(vote1);
                votes.Add(vote2);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var voteRequest = new DeleteVoteRequestModel { OptionNumber = pollOptionNumber1 };
                var ballotRequest = new DeleteBallotRequestModel { BallotManageGuid = PollManageGuid };
                ballotRequest.VoteDeleteRequests.Add(voteRequest);

                var request = new DeleteVotersRequestModel();
                request.BallotDeleteRequests.Add(ballotRequest);


                controller.Delete(PollManageGuid, request);


                Assert.AreEqual(1, ballots.Count());
                Assert.AreEqual(1, poll.Ballots.Count());
            }

            [TestMethod]
            public void OnlyRequestedVotesRemoved()
            {
                const int pollOptionNumber1 = 1;
                const int pollOptionNumber2 = 5;

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreatePoll();

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot()
                {
                    ManageGuid = PollManageGuid
                };

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote()
                {
                    Option = new Option()
                    {
                        PollOptionNumber = pollOptionNumber1
                    }
                };
                var vote2 = new Vote()
                {
                    Option = new Option()
                    {
                        PollOptionNumber = pollOptionNumber2
                    }
                };


                ballot.Votes.Add(vote1);
                ballot.Votes.Add(vote2);
                poll.Ballots.Add(ballot);

                votes.Add(vote1);
                votes.Add(vote2);
                ballots.Add(ballot);
                polls.Add(poll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes);
                ManageVoterController controller = CreateManageVoteController(contextFactory);

                var voteRequest = new DeleteVoteRequestModel { OptionNumber = pollOptionNumber1 };
                var ballotRequest = new DeleteBallotRequestModel { BallotManageGuid = PollManageGuid };
                ballotRequest.VoteDeleteRequests.Add(voteRequest);

                var request = new DeleteVotersRequestModel();
                request.BallotDeleteRequests.Add(ballotRequest);


                controller.Delete(PollManageGuid, request);


                Assert.AreEqual(1, votes.Count());
                Assert.AreEqual(1, ballot.Votes.Count());
            }
        }

        public static ManageVoterController CreateManageVoteController(IContextFactory contextFactory)
        {
            var metricHandler = new Mock<IMetricHandler>();
            return CreateManageVoteController(contextFactory, metricHandler.Object);
        }

        public static ManageVoterController CreateManageVoteController(IContextFactory contextFactory, IMetricHandler metricHandler)
        {
            return new ManageVoterController(contextFactory, metricHandler)
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
