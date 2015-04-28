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
            Mock<IVotingContext> mockContext = CreateMockContext();

            MockOutPolls(mockContext, polls);

            return CreateMockFactory(mockContext);
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Option> options)
        {
            Mock<IVotingContext> mockContext = CreateMockContext();

            MockOutPolls(mockContext, polls);
            MockOutOptions(mockContext, options);

            return CreateMockFactory(mockContext);
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Ballot> ballots)
        {
            Mock<IVotingContext> mockContext = CreateMockContext();

            MockOutPolls(mockContext, polls);
            MockOutBallots(mockContext, ballots);

            return CreateMockFactory(mockContext);
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Ballot> ballots, IDbSet<Vote> votes)
        {
            Mock<IVotingContext> mockContext = CreateMockContext();

            MockOutPolls(mockContext, polls);
            MockOutBallots(mockContext, ballots);
            MockOutVotes(mockContext, votes);

            return CreateMockFactory(mockContext);
        }

        public static IContextFactory CreateContextFactory(IDbSet<Poll> polls, IDbSet<Ballot> ballots, IDbSet<Vote> votes, IDbSet<Option> options)
        {
            Mock<IVotingContext> mockContext = CreateMockContext();

            MockOutPolls(mockContext, polls);
            MockOutBallots(mockContext, ballots);
            MockOutVotes(mockContext, votes);
            MockOutOptions(mockContext, options);

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

        private static void MockOutOptions(Mock<IVotingContext> mockContext, IDbSet<Option> options)
        {
            mockContext
                .Setup(p => p.Options)
                .Returns(options);
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
