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
        private Guid _mainUUID;
        private Guid _otherUUID;
        private Guid _emptyUUID;
        private Guid _anonymousUUID;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Choice> dummyChoices = new InMemoryDbSet<Choice>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();
            _anonymousUUID = Guid.NewGuid();

            Poll mainPoll = new Poll() { UUID = _mainUUID, LastUpdatedUtc = DateTime.UtcNow };
            Poll otherPoll = new Poll() { UUID = _otherUUID };
            Poll emptyPoll = new Poll() { UUID = _emptyUUID };
            Poll anonymousPoll = new Poll() { UUID = _anonymousUUID };

            anonymousPoll.NamedVoting = false;

            _burgerChoice = new Choice { Id = 1, Name = "Burger King" };

            _bobVote = new Vote() { Id = 1, Poll = mainPoll, Choice = _burgerChoice, Ballot = new Ballot(), VoteValue = 1 };
            _joeVote = new Vote() { Id = 2, Poll = mainPoll, Choice = _burgerChoice, Ballot = new Ballot(), VoteValue = 1 };
            _otherVote = new Vote() { Id = 3, Poll = new Poll() { UUID = _otherUUID }, Choice = new Choice() { Id = 1 }, Ballot = new Ballot() };
            _anonymousVote = new Vote() { Id = 4, Poll = new Poll() { UUID = _anonymousUUID }, Choice = new Choice() { Id = 1 }, Ballot = new Ballot() };

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

            _controller = new PollResultsController(mockContextFactory.Object, mockMetricHandler.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _mainUUID
            };

            // Act
            _controller.Get(request);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = newGuid
            };

            _controller.Get(request);
        }

        [TestMethod]
        public void GetReturnsVotesForThatPoll()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _mainUUID
            };

            // Act
            var response = _controller.Get(request);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(2, responseVotes.Count);
            CollectionAssert.AreEqual(new long[] { 1, 1 }, responseVotes.Select(r => r.ChoiceId).ToArray());
        }

        [TestMethod]
        public void GetOnEmptyPollReturnsEmptyList()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _emptyUUID
            };

            // Act
            var response = _controller.Get(request);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(0, responseVotes.Count);
        }

        [TestMethod]
        public void GetOnAnonPollDoesNotReturnUsernames()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _anonymousUUID
            };

            // Act
            var response = _controller.Get(request);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(1, responseVotes.Count);
            Assert.AreEqual("Anonymous User", responseVotes[0].VoterName);
        }

        [TestMethod]
        public void GetWithLowTimestampReturnsResults()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _mainUUID
            };

            // Act
            Uri requestURI;
            Uri.TryCreate("http://localhost/?lastRefreshed=0", UriKind.Absolute, out requestURI);
            _controller.Request.RequestUri = requestURI;
            var response = _controller.Get(request);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(2, responseVotes.Count);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotModified)]
        public void GetWithHighTimestampReturnsNotModified()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _mainUUID
            };

            // Act
            Uri requestURI;
            Uri.TryCreate("http://localhost/?lastRefreshed=2145916800000", UriKind.Absolute, out requestURI);
            _controller.Request.RequestUri = requestURI;
            var response = _controller.Get(request);
        }

        [TestMethod]
        public void GetReturnsWinnerForThatPoll()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _mainUUID
            };

            // Act
            var response = _controller.Get(request);

            // Assert
            List<Choice> responseWinners = response.Winners;
            List<Choice> expectedWinners = new List<Choice>() { _burgerChoice };

            Assert.AreEqual(1, responseWinners.Count);

            CollectionAssert.AreEquivalent(expectedWinners, responseWinners);
        }

        [TestMethod]
        public void GetReturnsSummaryForThatPoll()
        {
            // Arrange
            ResultsRequestModel request = new ResultsRequestModel()
            {
                BallotGuid = _mainUUID
            };

            // Act
            var response = _controller.Get(request);

            // Assert
            List<ResultModel> responseResults = response.Results;

            Assert.AreEqual(1, responseResults.Count);
            Assert.AreEqual(2, responseResults[0].Voters.Count);
            Assert.AreEqual(_burgerChoice, responseResults[0].Choice);
            Assert.AreEqual(2, responseResults[0].Sum);
        }

        #endregion

        [TestClass]
        public class GetTests
        {
            private readonly Guid _pollManageGuid = new Guid("BD7F7BB2-0CDC-4CFC-93E9-1F34C519C544");

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
                    UUID = _pollManageGuid,
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

                var controller = new PollResultsController(contextFactory, Mock.Of<IMetricHandler>())
                {
                    Request = new HttpRequestMessage()
                };

                ResultsRequestModel request = new ResultsRequestModel()
                {
                    BallotGuid = _pollManageGuid
                };

                ResultsRequestResponseModel response = controller.Get(request);


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
                    UUID = _pollManageGuid,
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

                var controller = new PollResultsController(contextFactory, Mock.Of<IMetricHandler>())
                {
                    Request = new HttpRequestMessage()
                };

                ResultsRequestModel request = new ResultsRequestModel()
                {
                    BallotGuid = _pollManageGuid
                };

                ResultsRequestResponseModel response = controller.Get(request);


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
                    UUID = _pollManageGuid,
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

                var controller = new PollResultsController(contextFactory, Mock.Of<IMetricHandler>())
                {
                    Request = new HttpRequestMessage()
                };

                ResultsRequestModel request = new ResultsRequestModel()
                {
                    BallotGuid = _pollManageGuid
                };

                ResultsRequestResponseModel response = controller.Get(request);


                var expectedVoterNames = new List<string>() { "Bob", "Bob", "Anonymous Voter" };

                List<string> voterNames = response
                    .Results
                    .SelectMany(r => r.Voters)
                    .Select(v => v.Name)
                    .ToList();

                CollectionAssert.AreEquivalent(expectedVoterNames, voterNames);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.Forbidden)]
            public void ElectionMode_True_ThrowsExceptionForInvalid()
            {
                /* poll
                      ballot1
                        vote1   (option 1)
                 */

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                var poll = new Poll()
                {
                    UUID = _pollManageGuid,
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
                ballots.Add(ballot1);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Choice = option1, Poll = poll, Ballot = ballot1 };
                votes.Add(vote1);

                ballot1.Votes.Add(vote1);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots, votes, options);

                var controller = new PollResultsController(contextFactory, Mock.Of<IMetricHandler>())
                {
                    Request = new HttpRequestMessage()
                };

                ResultsRequestModel request = new ResultsRequestModel()
                {
                    BallotGuid = _pollManageGuid
                };

                ResultsRequestResponseModel response = controller.Get(request);


                List<ResultVoteModel> voters = response
                    .Results
                    .SelectMany(r => r.Voters)
                    .ToList();

                Assert.IsTrue(voters.All(v => v.Name == null));
            }
        }
    }
}
