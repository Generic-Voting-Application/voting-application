using FakeDbSet;
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
using VotingApplication.Web.Api.Controllers;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Tests.TestHelpers;

namespace VotingApplication.Web.Tests.Controllers
{
    [TestClass]
    public class PollResultControllerTests
    {
        private PollResultsController _controller;
        private Vote _bobVote;
        private Vote _joeVote;
        private Vote _otherVote;
        private Vote _anonymousVote;
        private Choice _burgerChoice;
        private Guid _mainUuid;
        private Guid _otherUuid;
        private Guid _emptyUuid;
        private Guid _anonymousUuid;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void Setup()
        {
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Choice> dummyChoices = new InMemoryDbSet<Choice>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            _mainUuid = Guid.NewGuid();
            _otherUuid = Guid.NewGuid();
            _emptyUuid = Guid.NewGuid();
            _anonymousUuid = Guid.NewGuid();

            Poll mainPoll = new Poll() { UUID = _mainUuid, LastUpdatedUtc = DateTime.UtcNow };
            Poll otherPoll = new Poll() { UUID = _otherUuid };
            Poll emptyPoll = new Poll() { UUID = _emptyUuid };
            Poll anonymousPoll = new Poll { UUID = _anonymousUuid, NamedVoting = false };


            _burgerChoice = new Choice { Id = 1, Name = "Burger King" };

            _bobVote = new Vote() { Id = 1, Poll = mainPoll, Choice = _burgerChoice, Ballot = new Ballot(), VoteValue = 1 };
            _joeVote = new Vote() { Id = 2, Poll = mainPoll, Choice = _burgerChoice, Ballot = new Ballot(), VoteValue = 1 };
            _otherVote = new Vote() { Id = 3, Poll = new Poll() { UUID = _otherUuid }, Choice = new Choice() { Id = 1 }, Ballot = new Ballot() };
            _anonymousVote = new Vote() { Id = 4, Poll = new Poll() { UUID = _anonymousUuid }, Choice = new Choice() { Id = 1 }, Ballot = new Ballot() };

            _dummyVotes.Add(_bobVote);
            _dummyVotes.Add(_joeVote);
            _dummyVotes.Add(_otherVote);
            _dummyVotes.Add(_anonymousVote);

            dummyChoices.Add(_burgerChoice);

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(emptyPoll);
            dummyPolls.Add(anonymousPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Choices).Returns(dummyChoices);
            mockContext.Setup(a => a.Polls).Returns(dummyPolls);

            var mockMetricHandler = new Mock<IMetricHandler>();

            _controller = new PollResultsController(mockContextFactory.Object, mockMetricHandler.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotModified)]
        public void GetWithHighTimestampReturnsNotModified()
        {
            // Act
            Uri requestUri;
            Uri.TryCreate("http://localhost/?lastRefreshed=2145916800000", UriKind.Absolute, out requestUri);
            _controller.Request.RequestUri = requestUri;
            _controller.Get(_mainUuid);
        }

        [TestMethod]
        public void GetReturnsSummaryForThatPoll()
        {
            // Act
            var response = _controller.Get(_mainUuid);

            // Assert
            List<ResultModel> responseResults = response.Results;

            Assert.AreEqual(1, responseResults.Count);
            Assert.AreEqual(2, responseResults[0].Voters.Count);
            Assert.AreEqual(_burgerChoice, responseResults[0].Choice);
            Assert.AreEqual(2, responseResults[0].Sum);
        }

        [TestClass]
        public class GetTests
        {
            public readonly Guid PollId = new Guid("16EA9A4A-7409-462B-BD0F-7274896FE6B5");
            public readonly Guid PollManageGuid = new Guid("BD7F7BB2-0CDC-4CFC-93E9-1F34C519C544");
            public readonly Guid TokenGuid = new Guid("2D5A994D-5CC6-49D9-BCF0-AD1D5D2A3739");

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void NonExistentPoll_ThrowsNotFound()
            {
                Guid unknownPollId = new Guid("0D5C94A2-F219-4327-9EED-4DCE2ECFAB6A");

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollResultsController controller = CreatePollController(contextFactory);


                controller.Get(unknownPollId);
            }

            [TestMethod]
            public void NamedVoting_False_ReturnsNullForVoterName()
            {
                /* poll
                      ballot1
                        vote1   (option 1)
                        vote2   (option 2)
                    ballot2
                        vote3   (option 1)
                 */

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    UUID = PollManageGuid,
                    NamedVoting = false
                };
                polls.Add(poll);

                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                var option1 = new Choice() { PollChoiceNumber = 1 };
                var option2 = new Choice() { PollChoiceNumber = 2 };
                options.Add(option1);
                options.Add(option2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot() { VoterName = "Barbara" };
                var ballot2 = new Ballot() { VoterName = "Doris" };
                ballots.Add(ballot1);
                ballots.Add(ballot2);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Choice = option1, Poll = poll, Ballot = ballot1 };
                var vote2 = new Vote() { Choice = option2, Poll = poll, Ballot = ballot1 };
                var vote3 = new Vote() { Choice = option1, Poll = poll, Ballot = ballot2 };
                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);

                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);
                ballot2.Votes.Add(vote3);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                PollResultsController controller = CreatePollController(contextFactory);


                ResultsRequestResponseModel response = controller.Get(PollManageGuid);


                List<ResultVoteModel> voters = response
                    .Results
                    .SelectMany(r => r.Voters)
                    .ToList();

                Assert.IsTrue(voters.All(v => v.Name == null));
            }

            [TestMethod]
            public void NamedVoting_True_ReturnsNotNullForVoterName()
            {
                /* poll
                      ballot1
                        vote1   (option 1)
                        vote2   (option 2)
                    ballot2
                        vote3   (option 1)
                 */

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    UUID = PollManageGuid,
                    NamedVoting = true
                };
                polls.Add(poll);

                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                var option1 = new Choice() { PollChoiceNumber = 1 };
                var option2 = new Choice() { PollChoiceNumber = 2 };
                options.Add(option1);
                options.Add(option2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot() { VoterName = "Barbara" };
                var ballot2 = new Ballot() { VoterName = "Doris" };
                ballots.Add(ballot1);
                ballots.Add(ballot2);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Choice = option1, Poll = poll, Ballot = ballot1 };
                var vote2 = new Vote() { Choice = option2, Poll = poll, Ballot = ballot1 };
                var vote3 = new Vote() { Choice = option1, Poll = poll, Ballot = ballot2 };
                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);

                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);
                ballot2.Votes.Add(vote3);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                PollResultsController controller = CreatePollController(contextFactory);


                ResultsRequestResponseModel response = controller.Get(PollManageGuid);


                List<ResultVoteModel> voters = response
                    .Results
                    .SelectMany(r => r.Voters)
                    .ToList();

                Assert.IsTrue(voters.All(v => v.Name != null));
            }

            [TestMethod]
            public void NamedVoting_True_WithPreviousAnonymousVotes_ReturnsAnonymousVoterForUnNamedVoters()
            {
                /*  poll
                        ballot1 [VoterName: Bob]
                            vote1   (option 1)
                            vote2   (option 2)  
                        ballot2 [VoterName: null]
                            vote3   (option 1)
                 */

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    UUID = PollManageGuid,
                    NamedVoting = true
                };
                polls.Add(poll);

                IDbSet<Choice> options = DbSetTestHelper.CreateMockDbSet<Choice>();
                var option1 = new Choice() { PollChoiceNumber = 1 };
                var option2 = new Choice() { PollChoiceNumber = 2 };
                options.Add(option1);
                options.Add(option2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot() { VoterName = "Bob" };
                var ballot2 = new Ballot() { VoterName = null };
                ballots.Add(ballot1);
                ballots.Add(ballot2);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Choice = option1, Poll = poll, Ballot = ballot1 };
                var vote2 = new Vote() { Choice = option2, Poll = poll, Ballot = ballot1 };
                var vote3 = new Vote() { Choice = option1, Poll = poll, Ballot = ballot2 };
                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);

                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);
                ballot2.Votes.Add(vote3);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                PollResultsController controller = CreatePollController(contextFactory);


                ResultsRequestResponseModel response = controller.Get(PollManageGuid);


                var expectedVoterNames = new List<string>() { "Bob", "Bob", "Anonymous Voter" };

                List<string> voterNames = response
                    .Results
                    .SelectMany(r => r.Voters)
                    .Select(v => v.Name)
                    .ToList();

                CollectionAssert.AreEquivalent(expectedVoterNames, voterNames);
            }

            [TestMethod]
            public void NonInviteOnly_NoXTokenGuidHeader_SingleWinner_ReturnsWinner()
            {
                const int winnerPollChoiceNumber = 1;
                const string winnerName = "Winner-winner-chicken-dinner";
                const string winnerDescription = "A winner is you!";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreateNonInviteOnlyPoll();
                polls.Add(poll);

                IDbSet<Choice> choices = DbSetTestHelper.CreateMockDbSet<Choice>();
                var winningChoice = new Choice()
                {
                    Name = winnerName,
                    PollChoiceNumber = winnerPollChoiceNumber,
                    Description = winnerDescription
                };
                var losingChoice = new Choice() { PollChoiceNumber = 2 };
                choices.Add(winningChoice);
                choices.Add(losingChoice);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot() { VoterName = "Barbara" };
                ballots.Add(ballot);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote() { Choice = winningChoice, Poll = poll, Ballot = ballot };

                votes.Add(vote);
                ballot.Votes.Add(vote);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, choices);
                PollResultsController controller = CreatePollController(contextFactory);


                ResultsRequestResponseModel response = controller.Get(PollId);


                Assert.AreEqual(1, response.Winners.Count);

                string winner = response.Winners.First();
                Assert.AreEqual(winnerName, winner);
            }

            [TestMethod]
            public void NonInviteOnly_NoXTokenGuidHeader_MultipleWinner_ReturnsWinners()
            {
                const int winner1PollChoiceNumber = 1;
                const int winner2PollChoiceNumber = 2;
                const string winner1Name = "Winner-winner-chicken-dinner";
                const string winner2Name = "Winning";
                const string winner1Description = "A winner is you!";
                const string winner2Description = "";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreateNonInviteOnlyPoll();
                polls.Add(poll);

                IDbSet<Choice> choices = DbSetTestHelper.CreateMockDbSet<Choice>();
                var winningChoice1 = new Choice()
                {
                    Name = winner1Name,
                    PollChoiceNumber = winner1PollChoiceNumber,
                    Description = winner1Description,
                    Id = 1
                };
                var winningChoice2 = new Choice()
                {
                    Name = winner2Name,
                    PollChoiceNumber = winner2PollChoiceNumber,
                    Description = winner2Description,
                    Id = 2
                };
                choices.Add(winningChoice1);
                choices.Add(winningChoice2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot() { VoterName = "Barbara" };
                ballots.Add(ballot);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Choice = winningChoice1, Poll = poll, Ballot = ballot };
                var vote2 = new Vote() { Choice = winningChoice2, Poll = poll, Ballot = ballot };

                votes.Add(vote1);
                votes.Add(vote2);
                ballot.Votes.Add(vote1);
                ballot.Votes.Add(vote2);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, choices);
                PollResultsController controller = CreatePollController(contextFactory);


                ResultsRequestResponseModel response = controller.Get(PollId);


                Assert.AreEqual(2, response.Winners.Count);

                string winner1 = response.Winners.First();
                Assert.AreEqual(winner1Name, winner1);

                string winner2 = response.Winners.Last();
                Assert.AreEqual(winner2Name, winner2);

            }

            [TestMethod]
            public void NonInviteOnly_NoXTokenGuidHeader_ReturnsResults()
            {
                const int pollChoiceNumber1 = 1;
                const int pollChoiceNumber2 = 2;
                const string name1 = "Winner-winner-chicken-dinner";
                const string name2 = "Some other option";
                const string description1 = "A winner is you!";
                const string description2 = "Loser";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreateNonInviteOnlyPoll();
                polls.Add(poll);

                IDbSet<Choice> choices = DbSetTestHelper.CreateMockDbSet<Choice>();
                var choice1 = new Choice()
                {
                    Name = name1,
                    PollChoiceNumber = pollChoiceNumber1,
                    Description = description1,
                    Id = 1
                };
                var choice2 = new Choice()
                {
                    Name = name2,
                    PollChoiceNumber = pollChoiceNumber2,
                    Description = description2,
                    Id = 2
                };
                choices.Add(choice1);
                choices.Add(choice2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot() { VoterName = "Barbara" };
                var ballot2 = new Ballot() { VoterName = "Derek" };
                ballots.Add(ballot1);
                ballots.Add(ballot2);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = CreateVote(choice1, poll, ballot1);
                var vote2 = CreateVote(choice2, poll, ballot1);

                var vote3 = CreateVote(choice1, poll, ballot2);

                votes.Add(vote1);
                votes.Add(vote2);
                votes.Add(vote3);

                ballot1.Votes.Add(vote1);
                ballot1.Votes.Add(vote2);

                ballot2.Votes.Add(vote3);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, choices);
                PollResultsController controller = CreatePollController(contextFactory);


                ResultsRequestResponseModel response = controller.Get(PollId);


                Assert.AreEqual(2, response.Results.Count);

                var result1 = response.Results.First();
                Assert.AreEqual(name1, result1.Choice.Name);
                Assert.AreEqual(pollChoiceNumber1, result1.Choice.PollChoiceNumber);
                Assert.AreEqual(description1, result1.Choice.Description);

                Assert.AreEqual(2, result1.Sum);
                Assert.AreEqual(2, result1.Voters.Count);

                var result2 = response.Results.Last();
                Assert.AreEqual(name2, result2.Choice.Name);
                Assert.AreEqual(pollChoiceNumber2, result2.Choice.PollChoiceNumber);
                Assert.AreEqual(description2, result2.Choice.Description);

                Assert.AreEqual(1, result2.Sum);
                Assert.AreEqual(1, result2.Voters.Count);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NonInviteOnly_MultipleXTokenGuidHeader_ThrowsBadRequest()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateNonInviteOnlyPoll());
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollResultsController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);
                AddXTokenGuidHeader(controller, TokenGuid);


                controller.Get(PollId);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void NonInviteOnly_XTokenGuidHeader_BallotNotInPoll_ThrowsNotFound()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateNonInviteOnlyPoll());
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollResultsController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                controller.Get(PollId);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
            public void InviteOnly_NoXTokenGuidHeader_ThrowsForbidden()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateInviteOnlyPoll());
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollResultsController controller = CreatePollController(contextFactory);

                controller.Get(PollId);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
            public void InviteOnly_XTokenGuidHeader_BallotNotInPoll_ThrowsForbidden()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateInviteOnlyPoll());
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollResultsController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                controller.Get(PollId);
            }

            [TestMethod]
            public void InviteOnly_XTokenGuidHeader_BallotInPoll_ReturnsWinners()
            {
                const int winnerPollChoiceNumber = 1;
                const string winnerName = "Winner-winner-chicken-dinner";
                const string winnerDescription = "A winner is you!";

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = CreateInviteOnlyPoll();
                polls.Add(poll);

                IDbSet<Choice> choices = DbSetTestHelper.CreateMockDbSet<Choice>();
                var winningChoice = new Choice()
                {
                    Name = winnerName,
                    PollChoiceNumber = winnerPollChoiceNumber,
                    Description = winnerDescription
                };
                var losingChoice = new Choice() { PollChoiceNumber = 2 };
                choices.Add(winningChoice);
                choices.Add(losingChoice);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot = new Ballot() { VoterName = "Barbara", TokenGuid = TokenGuid };
                ballots.Add(ballot);
                poll.Ballots.Add(ballot);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote = new Vote() { Choice = winningChoice, Poll = poll, Ballot = ballot };

                votes.Add(vote);
                ballot.Votes.Add(vote);


                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, choices);
                PollResultsController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                ResultsRequestResponseModel response = controller.Get(PollId);


                Assert.AreEqual(1, response.Winners.Count);

                string winner = response.Winners.First();
                Assert.AreEqual(winnerName, winner);
            }

            private Poll CreateNonInviteOnlyPoll()
            {
                return new Poll() { UUID = PollId, InviteOnly = false };
            }

            private Poll CreateInviteOnlyPoll()
            {
                return new Poll() { UUID = PollId, InviteOnly = true };
            }

            private static Vote CreateVote(Choice choice, Poll poll, Ballot ballot)
            {
                return new Vote() { Choice = choice, Poll = poll, Ballot = ballot, VoteValue = 1 };
            }

            public static PollResultsController CreatePollController(IContextFactory contextFactory)
            {
                return new PollResultsController(contextFactory, null)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }

            public static void AddXTokenGuidHeader(ApiController controller, Guid tokenGuid)
            {
                controller.Request.Headers.Add("X-TokenGuid", tokenGuid.ToString("D"));
            }
        }
    }
}
