using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
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
    public class PollControllerTests
    {
        const string UserId = "4AEAE121-D540-48BF-907A-AA454248C0C0";

        private PollController _controller;
        private Mock<IMetricHandler> _metricHandler;
        private Poll _mainPoll;
        private Poll _otherPoll;
        private Poll _templatePoll;
        private Guid _templateUUID;
        private DateTime _templateCreatedDate;
        private Guid[] UUIDs;
        private Choice _redChoice;
        private InMemoryDbSet<Poll> _dummyPolls;

        [TestInitialize]
        public void setup()
        {
            _redChoice = new Choice() { Name = "Red" };

            UUIDs = new[] { Guid.NewGuid(), Guid.NewGuid(), _templateUUID, Guid.NewGuid() };
            _mainPoll = new Poll() { UUID = UUIDs[0], ManageId = Guid.NewGuid() };
            _otherPoll = new Poll() { UUID = UUIDs[1], ManageId = Guid.NewGuid() };

            _templateUUID = Guid.NewGuid();
            _templateCreatedDate = DateTime.UtcNow.AddDays(-5);
            _templatePoll = new Poll()
            {
                UUID = _templateUUID,
                ManageId = Guid.NewGuid(),
                CreatedDateUtc = _templateCreatedDate,
                Choices = new List<Choice>() { _redChoice },
                CreatorIdentity = UserId
            };

            _dummyPolls = new InMemoryDbSet<Poll>(true) { _mainPoll, _otherPoll, _templatePoll };

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Polls).Returns(_dummyPolls);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _metricHandler = new Mock<IMetricHandler>();

            _controller = new PollController(mockContextFactory.Object, _metricHandler.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyPolls.Local.Count; i++)
            {
                _dummyPolls.Local[i].UUID = UUIDs[i];
            }
        }

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            _controller.Post(new PollCreationRequestModel() { PollName = "New Poll" });
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void PostRejectsPollWithInvalidInput()
        {
            // Arrange
            _controller.ModelState.AddModelError("PollName", "");

            // Act
            _controller.Post(new PollCreationRequestModel());
        }

        [TestMethod]
        public void PostAssignsPollUUID()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, response.UUID);
        }

        [TestMethod]
        public void PostAssignsPollManageId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(Guid.Empty, response.ManageId);
        }

        [TestMethod]
        public void PostAssignsPollManageIdDifferentFromPollId()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreNotEqual(response.UUID, response.ManageId);
        }

        [TestMethod]
        public void PostWithAuthorizationSetsUsernameOfPollOwner()
        {
            // Mocking of GetUserId() taken from http://stackoverflow.com/questions/22762338/how-do-i-mock-user-identity-getuserid

            var claim = new Claim("test", UserId);
            var mockIdentity = new Mock<ClaimsIdentity>();
            mockIdentity
                .Setup(ci => ci.FindFirst(It.IsAny<string>()))
                .Returns(claim);

            mockIdentity
                .Setup(i => i.IsAuthenticated)
                .Returns(true);

            var principal = new Mock<IPrincipal>();
            principal
                .Setup(ip => ip.Identity)
                .Returns(mockIdentity.Object);

            _controller.User = principal.Object;


            PollCreationRequestModel newPoll = new PollCreationRequestModel()
            {
                PollName = "New Poll"
            };


            _controller.Post(newPoll);


            Poll createdPoll = _dummyPolls.Last();

            Assert.AreEqual(UserId, createdPoll.CreatorIdentity);
        }

        [TestMethod]
        public void PostReturnsIDsOfNewPoll()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreEqual(_dummyPolls.Last().UUID, response.UUID);
            Assert.AreEqual(_dummyPolls.Last().ManageId, response.ManageId);
        }

        [TestMethod]
        public void PostAddsNewPollToPolls()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            Assert.AreEqual(_dummyPolls.Count(), 4);
        }

        [TestMethod]
        public void SuccessfulPollCreationGeneratesMetric()
        {
            // Act
            PollCreationRequestModel newPoll = new PollCreationRequestModel() { PollName = "New Poll" };
            var response = _controller.Post(newPoll);

            // Assert
            _metricHandler.Verify(m => m.HandlePollCreatedEvent(It.Is<Poll>(p => p.Name == "New Poll" && p.UUID == response.UUID)), Times.Once());
        }

        [TestMethod]
        [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
        public void UnsuccessfulPollCreationDoesNotGenerateMetric()
        {
            // Act
            var response = _controller.Post(null);

            // Assert
            _metricHandler.Verify(m => m.HandlePollCreatedEvent(It.IsAny<Poll>()), Times.Never());
        }

        #endregion

        [TestClass]
        public class GetTests
        {
            public readonly Guid PollId = new Guid("5423A511-2BA7-4918-BA7D-04024FC18669");

            public readonly Guid TokenGuid = new Guid("2D5A994D-5CC6-49D9-BCF0-AD1D5D2A3739");

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.NotFound)]
            public void NonExistentPoll_ThrowsNotFound()
            {
                Guid unknownPollId = new Guid("0D5C94A2-F219-4327-9EED-4DCE2ECFAB6A");

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollController controller = CreatePollController(contextFactory);


                controller.Get(unknownPollId);
            }

            [TestMethod]
            public void NonInviteOnly_NoXTokenGuidHeader_ReturnsPoll()
            {
                const string pollName = "Why are we here?";
                const PollType pollType = PollType.UpDown;

                var poll = new Poll()
                {
                    UUID = PollId,
                    Name = pollName,
                    PollType = pollType,

                    MaxPoints = 5,
                    MaxPerVote = 1,

                    ExpiryDateUtc = null,

                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = false,
                    InviteOnly = false
                };

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(poll);
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollController controller = CreatePollController(contextFactory);

                PollRequestResponseModel response = controller.Get(PollId);


                Assert.AreEqual(pollName, response.Name);
                Assert.AreEqual(pollType.ToString(), response.PollType);
                Assert.IsNull(response.ExpiryDateUtc);

                Assert.AreEqual(5, response.MaxPoints);
                Assert.AreEqual(1, response.MaxPerVote);

                Assert.IsFalse(response.NamedVoting);
                Assert.IsFalse(response.ChoiceAdding);
                Assert.IsFalse(response.ElectionMode);
                Assert.IsFalse(response.UserHasVoted);
            }

            [TestMethod]
            public void NonInviteOnly_NoXTokenGuidHeader_ReturnsNonEmptyTokenGuid()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(new Poll() { UUID = PollId });
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollController controller = CreatePollController(contextFactory);


                PollRequestResponseModel response = controller.Get(PollId);

                Assert.AreNotEqual(Guid.Empty, response.TokenGuid);
            }

            [TestMethod]
            public void NonInviteOnly_XTokenGuidHeader_ReturnsHeaderValueAsTokenGuid()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                var ballot = new Ballot() { TokenGuid = TokenGuid };

                Poll nonInviteOnlyPoll = CreateNonInviteOnlyPoll();
                nonInviteOnlyPoll.Ballots.Add(ballot);

                ballots.Add(ballot);
                polls.Add(nonInviteOnlyPoll);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                PollRequestResponseModel response = controller.Get(PollId);

                Assert.AreEqual(TokenGuid, response.TokenGuid);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.BadRequest)]
            public void NonInviteOnly_MultipleXTokenGuidHeader_ThrowsBadRequest()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateNonInviteOnlyPoll());
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollController controller = CreatePollController(contextFactory);
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

                PollController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                controller.Get(PollId);
            }

            [TestMethod]
            public void NonInviteOnly_NoXTokenGuidHeader_AddsNewBallot()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();

                polls.Add(CreateNonInviteOnlyPoll());

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);
                PollController controller = CreatePollController(contextFactory);


                controller.Get(PollId);


                Assert.AreEqual(1, ballots.Count());
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.Unauthorized)]
            public void InviteOnly_NoXTokenGuidHeader_ThrowsUnauthorized()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateInviteOnlyPoll());
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollController controller = CreatePollController(contextFactory);

                controller.Get(PollId);
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.Unauthorized)]
            public void InviteOnly_NoXTokenGuidHeader_DoesNotCreateBallot()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateInviteOnlyPoll());
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);

                PollController controller = CreatePollController(contextFactory);

                controller.Get(PollId);

                Assert.AreEqual(0, ballots.Count());
            }

            [TestMethod]
            [ExpectedHttpResponseException(HttpStatusCode.Unauthorized)]
            public void InviteOnly_XTokenGuidHeader_BallotNotInPoll_ThrowsUnauthorized()
            {
                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(CreateInviteOnlyPoll());
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls);

                PollController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                controller.Get(PollId);
            }

            [TestMethod]
            public void InviteOnly_XTokenGuidHeader_BallotInPoll_ReturnsPoll()
            {
                const string pollName = "Why are we here?";
                const PollType pollType = PollType.UpDown;

                var ballot = new Ballot() { TokenGuid = TokenGuid };

                var poll = new Poll()
                {
                    UUID = PollId,
                    Name = pollName,
                    PollType = pollType,

                    MaxPoints = 5,
                    MaxPerVote = 1,

                    ExpiryDateUtc = null,

                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = false,
                    InviteOnly = true
                };
                poll.Ballots.Add(ballot);

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(poll);
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                ballots.Add(ballot);
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);

                PollController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                PollRequestResponseModel response = controller.Get(PollId);


                Assert.AreEqual(pollName, response.Name);
                Assert.AreEqual(pollType.ToString(), response.PollType);
                Assert.IsNull(response.ExpiryDateUtc);

                Assert.AreEqual(5, response.MaxPoints);
                Assert.AreEqual(1, response.MaxPerVote);

                Assert.IsFalse(response.NamedVoting);
                Assert.IsFalse(response.ChoiceAdding);
                Assert.IsFalse(response.ElectionMode);
                Assert.IsFalse(response.UserHasVoted);
            }

            [TestMethod]
            public void ElectionPoll_BallotHasVoted_UserHasVotedIsTrue()
            {
                const string pollName = "Why are we here?";
                const PollType pollType = PollType.Basic;

                var ballot = new Ballot() { TokenGuid = TokenGuid, HasVoted = true };

                var poll = new Poll()
                {
                    UUID = PollId,
                    Name = pollName,
                    PollType = pollType,

                    ExpiryDateUtc = null,

                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = false,
                    InviteOnly = true
                };
                poll.Ballots.Add(ballot);

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(poll);
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                ballots.Add(ballot);
                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);

                PollController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                PollRequestResponseModel response = controller.Get(PollId);

                Assert.IsTrue(response.UserHasVoted);
            }

            [TestMethod]
            public void InviteOnly_XTokenGuidHeader_BallotInPoll_ReturnsHeaderValueAsTokenGuid()
            {
                var ballot = new Ballot() { TokenGuid = TokenGuid };

                Poll poll = CreateInviteOnlyPoll();
                poll.Ballots.Add(ballot);

                IDbSet<Poll> polls = DbSetTestHelper.CreateMockDbSet<Poll>();
                polls.Add(poll);
                IDbSet<Ballot> ballots = DbSetTestHelper.CreateMockDbSet<Ballot>();
                ballots.Add(ballot);

                IContextFactory contextFactory = ContextFactoryTestHelper.CreateContextFactory(polls, ballots);

                PollController controller = CreatePollController(contextFactory);
                AddXTokenGuidHeader(controller, TokenGuid);


                controller.Get(PollId);
            }

            private Poll CreateNonInviteOnlyPoll()
            {
                return new Poll() { UUID = PollId, InviteOnly = false };
            }

            private Poll CreateInviteOnlyPoll()
            {
                return new Poll() { UUID = PollId, InviteOnly = true };
            }

            public static PollController CreatePollController(IContextFactory contextFactory)
            {
                return new PollController(contextFactory, null)
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
