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
        private Option _burgerOption;
        private Guid _mainUUID;
        private Guid _otherUUID;
        private Guid _emptyUUID;
        private Guid _anonymousUUID;
        private InMemoryDbSet<Vote> _dummyVotes;

        [TestInitialize]
        public void setup()
        {
            _dummyVotes = new InMemoryDbSet<Vote>(true);
            InMemoryDbSet<Option> dummyOptions = new InMemoryDbSet<Option>(true);
            InMemoryDbSet<Poll> dummyPolls = new InMemoryDbSet<Poll>(true);

            _mainUUID = Guid.NewGuid();
            _otherUUID = Guid.NewGuid();
            _emptyUUID = Guid.NewGuid();
            _anonymousUUID = Guid.NewGuid();

            Poll mainPoll = new Poll() { UUID = _mainUUID, LastUpdatedUtc = DateTime.Today };
            Poll otherPoll = new Poll() { UUID = _otherUUID };
            Poll emptyPoll = new Poll() { UUID = _emptyUUID };
            Poll anonymousPoll = new Poll() { UUID = _anonymousUUID };

            anonymousPoll.NamedVoting = false;

            _burgerOption = new Option { Id = 1, Name = "Burger King" };

            _bobVote = new Vote() { Id = 1, Poll = mainPoll, Option = _burgerOption, Ballot = new Ballot(), VoteValue = 1 };
            _joeVote = new Vote() { Id = 2, Poll = mainPoll, Option = _burgerOption, Ballot = new Ballot(), VoteValue = 1 };
            _otherVote = new Vote() { Id = 3, Poll = new Poll() { UUID = _otherUUID }, Option = new Option() { Id = 1 }, Ballot = new Ballot() };
            _anonymousVote = new Vote() { Id = 4, Poll = new Poll() { UUID = _anonymousUUID }, Option = new Option() { Id = 1 }, Ballot = new Ballot() };

            _dummyVotes.Add(_bobVote);
            _dummyVotes.Add(_joeVote);
            _dummyVotes.Add(_otherVote);
            _dummyVotes.Add(_anonymousVote);

            dummyOptions.Add(_burgerOption);

            dummyPolls.Add(mainPoll);
            dummyPolls.Add(otherPoll);
            dummyPolls.Add(emptyPoll);
            dummyPolls.Add(anonymousPoll);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Votes).Returns(_dummyVotes);
            mockContext.Setup(a => a.Options).Returns(dummyOptions);
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
            // Act
            _controller.Get(_mainUUID);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
        public void GetNonexistentPollIsNotFound()
        {
            // Act
            Guid newGuid = Guid.NewGuid();
            _controller.Get(newGuid);
        }

        [TestMethod]
        public void GetReturnsVotesForThatPoll()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(2, responseVotes.Count);
            CollectionAssert.AreEqual(new long[] { 1, 1 }, responseVotes.Select(r => r.OptionId).ToArray());
        }

        [TestMethod]
        public void GetOnEmptyPollReturnsEmptyList()
        {
            // Act
            var response = _controller.Get(_emptyUUID);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(0, responseVotes.Count);
        }

        [TestMethod]
        public void GetOnAnonPollDoesNotReturnUsernames()
        {
            // Act
            var response = _controller.Get(_anonymousUUID);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(1, responseVotes.Count);
            Assert.AreEqual("Anonymous User", responseVotes[0].VoterName);
        }

        [TestMethod]
        public void GetWithLowTimestampReturnsResults()
        {
            // Act
            Uri requestURI;
            Uri.TryCreate("http://localhost/?lastRefreshed=0", UriKind.Absolute, out requestURI);
            _controller.Request.RequestUri = requestURI;
            var response = _controller.Get(_mainUUID);

            // Assert
            var responseVotes = response.Votes;
            Assert.AreEqual(2, responseVotes.Count);
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.NotModified)]
        public void GetWithHighTimestampReturnsNotModified()
        {
            // Act
            Uri requestURI;
            Uri.TryCreate("http://localhost/?lastRefreshed=2145916800000", UriKind.Absolute, out requestURI);
            _controller.Request.RequestUri = requestURI;
            var response = _controller.Get(_mainUUID);
        }

        [TestMethod]
        public void GetReturnsWinnerForThatPoll()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            List<Option> responseWinners = response.Winners;
            List<Option> expectedWinners = new List<Option>() { _burgerOption };

            Assert.AreEqual(1, responseWinners.Count);

            CollectionAssert.AreEquivalent(expectedWinners, responseWinners);
        }

        [TestMethod]
        public void GetReturnsSummaryForThatPoll()
        {
            // Act
            var response = _controller.Get(_mainUUID);

            // Assert
            List<ResultModel> responseResults = response.Results;

            Assert.AreEqual(1, responseResults.Count);
            Assert.AreEqual(2, responseResults[0].Voters.Count);
            Assert.AreEqual(_burgerOption, responseResults[0].Option);
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

                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option1 = new Option() { PollOptionNumber = 1 };
                var option2 = new Option() { PollOptionNumber = 2 };
                options.Add(option1);
                options.Add(option2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot() { VoterName = "Barbara" };
                var ballot2 = new Ballot() { VoterName = "Doris" };
                ballots.Add(ballot1);
                ballots.Add(ballot2);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Option = option1, Poll = poll, Ballot = ballot1 };
                var vote2 = new Vote() { Option = option2, Poll = poll, Ballot = ballot1 };
                var vote3 = new Vote() { Option = option1, Poll = poll, Ballot = ballot2 };
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


                ResultsRequestResponseModel response = controller.Get(_pollManageGuid);


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

                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option1 = new Option() { PollOptionNumber = 1 };
                var option2 = new Option() { PollOptionNumber = 2 };
                options.Add(option1);
                options.Add(option2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot() { VoterName = "Barbara" };
                var ballot2 = new Ballot() { VoterName = "Doris" };
                ballots.Add(ballot1);
                ballots.Add(ballot2);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Option = option1, Poll = poll, Ballot = ballot1 };
                var vote2 = new Vote() { Option = option2, Poll = poll, Ballot = ballot1 };
                var vote3 = new Vote() { Option = option1, Poll = poll, Ballot = ballot2 };
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


                ResultsRequestResponseModel response = controller.Get(_pollManageGuid);


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

                IDbSet<Option> options = DbSetTestHelper.CreateMockDbSet<Option>();
                var option1 = new Option() { PollOptionNumber = 1 };
                var option2 = new Option() { PollOptionNumber = 2 };
                options.Add(option1);
                options.Add(option2);

                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                var ballot1 = new Ballot() { VoterName = "Bob" };
                var ballot2 = new Ballot() { VoterName = null };
                ballots.Add(ballot1);
                ballots.Add(ballot2);

                IDbSet<Vote> votes = DbSetTestHelper.CreateMockDbSet<Vote>();
                var vote1 = new Vote() { Option = option1, Poll = poll, Ballot = ballot1 };
                var vote2 = new Vote() { Option = option2, Poll = poll, Ballot = ballot1 };
                var vote3 = new Vote() { Option = option1, Poll = poll, Ballot = ballot2 };
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


                ResultsRequestResponseModel response = controller.Get(_pollManageGuid);


                var expectedVoterNames = new List<string>() { "Bob", "Bob", "Anonymous Voter" };

                List<string> voterNames = response
                    .Results
                    .SelectMany(r => r.Voters)
                    .Select(v => v.Name)
                    .ToList();

                CollectionAssert.AreEquivalent(expectedVoterNames, voterNames);
            }
        }
    }
}
