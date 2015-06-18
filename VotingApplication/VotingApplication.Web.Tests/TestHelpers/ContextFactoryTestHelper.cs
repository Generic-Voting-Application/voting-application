using Moq;
using System.Data.Entity;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Tests.TestHelpers
{
    public static class ContextFactoryTestHelper
    {
        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls)
        {
            return CreateContextFactory(polls, DbSetTestHelper.CreateMockDbSet<Ballot>());
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Choice> choices)
        {
            return CreateContextFactory(polls, DbSetTestHelper.CreateMockDbSet<Ballot>(), DbSetTestHelper.CreateMockDbSet<Vote>(), choices);
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Ballot> ballots)
        {
            return CreateContextFactory(polls, ballots, DbSetTestHelper.CreateMockDbSet<Vote>());
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Ballot> ballots, IDbSet<Vote> votes)
        {
            return CreateContextFactory(polls, ballots, votes, DbSetTestHelper.CreateMockDbSet<Choice>());
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Ballot> ballots, IDbSet<Vote> votes, IDbSet<Choice> choices)
        {
            Mock<IVotingContext> mockContext = CreateMockContext();

            MockOutPolls(mockContext, polls);
            MockOutBallots(mockContext, ballots);
            MockOutVotes(mockContext, votes);
            MockOutChoices(mockContext, choices);

            return CreateMockFactory(mockContext);
        }

        private static Mock<IVotingContext> CreateMockContext()
        {
            return new Mock<IVotingContext>();
        }

        private static void MockOutPolls(Mock<IVotingContext> mockContext, IDbSet<Poll> polls)
        {
            mockContext
                .Setup(p => p.Polls)
                .Returns(polls);
        }

        private static void MockOutBallots(Mock<IVotingContext> mockContext, IDbSet<Ballot> ballots)
        {
            mockContext
                .Setup(p => p.Ballots)
                .Returns(ballots);
        }

        private static void MockOutVotes(Mock<IVotingContext> mockContext, IDbSet<Vote> votes)
        {
            mockContext
                .Setup(p => p.Votes)
                .Returns(votes);
        }

        private static void MockOutChoices(Mock<IVotingContext> mockContext, IDbSet<Choice> choices)
        {
            mockContext
                .Setup(p => p.Choices)
                .Returns(choices);
        }

        private static IContextFactory CreateMockFactory(Mock<IVotingContext> mockContext)
        {
            var mockContextFactory = new Mock<IContextFactory>();

            mockContextFactory
                .Setup(a => a.CreateContext())
                .Returns(mockContext.Object);

            return mockContextFactory.Object;
        }
    }
}
