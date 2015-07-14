using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using VotingApplication.Data.Model;
using VotingApplication.Web.Tests.E2E.Helpers;
using VotingApplication.Web.Tests.E2E.Helpers.Clearers;

namespace VotingApplication.Web.Tests.E2E
{
    public class PointsPollTests
    {
        private const string ChromeDriverDir = @"..\..\";
        private const string SiteBaseUri = @"http://localhost:64205/";

        [TestClass]
        public class DefaultPollConfiguration : E2ETest
        {
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";

            [TestMethod]
            [TestCategory("E2E")]
            public void PopulatedChoices_DisplaysAllChoices()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> choiceNames = driver.FindElements(NgBy.Binding("choice.Name"));

                        Assert.AreEqual(poll.Choices.Count, choiceNames.Count);

                        List<string> expected = poll.Choices.Select(o => o.Name).ToList();
                        List<string> actual = choiceNames.Select(o => o.Text).ToList();
                        CollectionAssert.AreEquivalent(expected, actual);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void VotingOnChoice_NavigatesToResultsPage()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> increaseChoiceButtons = FindElementsById(driver, "increase-button");
                        increaseChoiceButtons.First().Click();

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();


                        string expectedUriPrefix = SiteBaseUri + "Poll/#/" + poll.UUID + "/Results";
                        Assert.IsTrue(driver.Url.StartsWith(expectedUriPrefix));
                    }
                }
            }

            [Ignore]
            [TestMethod]
            [TestCategory("E2E")]
            public void DefaultPoll_ShowsResultsButton()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IWebElement resultsLink = FindElementById(driver, "results-button");

                        Assert.IsTrue(resultsLink.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void PointsButtons_ChangePointAllocations()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> choices = driver.FindElements(NgBy.Repeater("choice in choices"));

                        IWebElement firstChoice = choices.First();
                        IWebElement selectedChoicePoints = firstChoice.FindElement(NgBy.Binding("choice.VoteValue"));

                        IReadOnlyCollection<IWebElement> increaseChoiceButtons = FindElementsById(driver, "increase-button");
                        IWebElement plusButton = increaseChoiceButtons.First();

                        IReadOnlyCollection<IWebElement> decreaseChoiceButtons = FindElementsById(driver, "decrease-button");
                        IWebElement minusButton = decreaseChoiceButtons.First();

                        string noPointsAllocatedPointsValue = "0 / " + poll.MaxPerVote;

                        Assert.AreEqual(noPointsAllocatedPointsValue, selectedChoicePoints.Text);

                        plusButton.Click();
                        string onePointAllocatedPointsValue = "1 / " + poll.MaxPerVote;
                        Assert.AreEqual(onePointAllocatedPointsValue, selectedChoicePoints.Text);

                        minusButton.Click();
                        Assert.AreEqual(noPointsAllocatedPointsValue, selectedChoicePoints.Text);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void PointsButtons_ChangeTotalPointLimit()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IWebElement totalPoints = FindElementById(driver, "points-remaining");

                        IReadOnlyCollection<IWebElement> increaseChoiceButtons = FindElementsById(driver, "increase-button");
                        IWebElement plusButton = increaseChoiceButtons.First();

                        IReadOnlyCollection<IWebElement> decreaseChoiceButtons = FindElementsById(driver, "decrease-button");
                        IWebElement minusButton = decreaseChoiceButtons.First();


                        string totalPointsNoneAllocated = poll.MaxPoints + " of " + poll.MaxPoints + " points remaining";
                        Assert.AreEqual(totalPointsNoneAllocated, totalPoints.Text);

                        plusButton.Click();
                        string totalPointsOneAllocated = (poll.MaxPoints - 1) + " of " + poll.MaxPoints + " points remaining";
                        Assert.AreEqual(totalPointsOneAllocated, totalPoints.Text);

                        minusButton.Click();
                        Assert.AreEqual(totalPointsNoneAllocated, totalPoints.Text);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void PointsPerChoice_CanNotExceedMaximum()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> increaseChoiceButtons = FindElementsById(driver, "increase-button");

                        IWebElement plusButton = increaseChoiceButtons.First();

                        // Max points per choice is 3 (See CreatePoll).
                        plusButton.Click();
                        plusButton.Click();
                        plusButton.Click();

                        Assert.IsFalse(plusButton.Enabled);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void TotalPoints_CanNotExceedMaximum()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollUrl);


                        IReadOnlyCollection<IWebElement> choices = driver.FindElements(NgBy.Repeater("choice in choices"));

                        // Allocate maximum points (3 - see CreatePoll) to the first choice.
                        IWebElement firstChoice = choices.First();
                        IWebElement firstIncreaseButton = firstChoice.FindElement(By.Id("increase-button"));

                        firstIncreaseButton.Click();
                        firstIncreaseButton.Click();
                        firstIncreaseButton.Click();

                        IWebElement secondChoice = choices.Last();
                        IWebElement secondIncreaseButton = secondChoice.FindElement(By.Id("increase-button"));

                        secondIncreaseButton.Click();


                        Assert.IsFalse(firstIncreaseButton.Enabled);
                        Assert.IsFalse(secondIncreaseButton.Enabled);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void PointsVote_AfterVoting_VotePointsAreCorrectAndVisible()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> choices = driver.FindElements(NgBy.Repeater("choice in choices"));

                        IWebElement choice = choices.First();
                        IWebElement increaseButton = choice.FindElement(By.Id("increase-button"));
                        increaseButton.Click();

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        NavigateBackToVotePage(driver);

                        IReadOnlyCollection<IWebElement> selectedChoices = driver.FindElements(NgBy.Repeater("choice in choices"));

                        IWebElement selectedChoice = selectedChoices.First();
                        IWebElement selectedChoicePoints = selectedChoice.FindElement(NgBy.Binding("poll.MaxPerVote"));


                        Assert.IsTrue(selectedChoicePoints.IsVisible());

                        string expectedChoicePoints = "1 / " + poll.MaxPerVote;
                        Assert.AreEqual(expectedChoicePoints, selectedChoicePoints.Text);
                    }
                }
            }

            public static Poll CreatePoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                    new Choice(){ Name = "Test Choice 2", Description = "Test Description 2" }
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                var defaultPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = false,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                testContext.Polls.Add(defaultPointsPoll);
                testContext.SaveChanges();

                return defaultPointsPoll;
            }

            private static void NavigateBackToVotePage(IWebDriver driver)
            {
                // The token is on the Uri, so we can't just navigate back to
                // PollUrl, as it won't display the selected vote.
                string resultsUri = driver.Url.Replace("Results", "Vote");
                GoToUrl(driver, resultsUri);
            }
        }

        [TestClass]
        public class InviteOnlyTests : E2ETest
        {
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";

            [TestMethod]
            [TestCategory("E2E")]
            public void AccessWithToken_DisplaysAllChoices()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);

                        GoToUrl(driver, PollUrl + "/" + poll.Ballots[0].TokenGuid);
                        IReadOnlyCollection<IWebElement> choiceNames = driver.FindElements(NgBy.Binding("choice.Name"));

                        Assert.AreEqual(poll.Choices.Count, choiceNames.Count);

                        List<string> expected = poll.Choices.Select(o => o.Name).ToList();
                        List<string> actual = choiceNames.Select(o => o.Text).ToList();
                        CollectionAssert.AreEquivalent(expected, actual);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void VoteWithToken_NavigatesToResultsPage()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);

                        GoToUrl(driver, PollUrl + "/" + poll.Ballots[0].TokenGuid);

                        IReadOnlyCollection<IWebElement> voteButtons = FindElementsById(driver, "vote-button");
                        voteButtons.First().Click();

                        string expectedUrlPrefix = SiteBaseUri + "Poll/#/" + poll.UUID + "/Results";
                        Assert.IsTrue(driver.Url.StartsWith(expectedUrlPrefix));
                    }
                }
            }

            public static Poll CreatePoll(TestVotingContext testContext)
            {
                List<Choice> testPollChoices = new List<Choice>
                {
                    new Choice() { Name = "Test Choice 1", Description = "Test Description 1"},
                    new Choice() { Name = "Test Choice 2", Description = "Test Description 2"}
                };


                // Invite Only, Anonymous, No Choice Adding, Shown Results
                var inviteOnlyPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = true,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = false,
                    MaxPerVote = 3,
                    MaxPoints = 4,

                    Ballots = new List<Ballot>()
                    {
                        new Ballot() { TokenGuid = Guid.NewGuid() }
                    }
                };

                testContext.Polls.Add(inviteOnlyPointsPoll);
                testContext.SaveChanges();

                return inviteOnlyPointsPoll;
            }
        }

        [TestClass]
        public class NamedVotingTests : E2ETest
        {
            const string VoterName = "User";
            private static readonly Guid PollGuid = Guid.NewGuid();
            private readonly string _pollVoteUrl = GetPollVoteUrl(PollGuid);
            private readonly string _pollResultsUrl = GetPollResultsUrl(PollGuid);

            [TestMethod]
            [TestCategory("E2E")]
            public void NoNameEntered_VoteNotAllowed()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, _pollVoteUrl);


                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        Assert.IsFalse(driver.Url.StartsWith(_pollResultsUrl));
                        Assert.IsTrue(driver.Url.StartsWith(_pollVoteUrl));
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void NameEntered_VoteAllowed()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, _pollVoteUrl);


                        IWebElement nameInput = FindElementById(driver, "voter-name-input");
                        nameInput.SendKeys(VoterName);

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        Assert.IsTrue(driver.Url.StartsWith(_pollResultsUrl));
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void NoNameEntered_ShowsFailedValidationMessage()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, _pollVoteUrl);


                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        IWebElement requiredValidationMessage = FindElementById(driver, "voter-name-required-validation-message");

                        Assert.IsTrue(requiredValidationMessage.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void EnteringVoterName_AllowsVoting()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, _pollVoteUrl);


                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        IWebElement requiredValidationMessage = FindElementById(driver, "voter-name-required-validation-message");

                        Assert.IsTrue(requiredValidationMessage.IsVisible());



                        IWebElement nameInput = FindElementById(driver, "voter-name-input");
                        nameInput.SendKeys(VoterName);

                        voteButton.Click();

                        Assert.IsTrue(driver.Url.StartsWith(_pollResultsUrl));
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void VotingAndReturning_RemembersVoterName()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, _pollVoteUrl);


                        IWebElement nameInput = FindElementById(driver, "voter-name-input");
                        nameInput.SendKeys(VoterName);

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        GoToUrl(driver, _pollVoteUrl);

                        IWebElement newNameInput = FindElementById(driver, "voter-name-input");

                        Assert.AreEqual(VoterName, newNameInput.GetAttribute("value"));
                    }
                }
            }

            public static Poll CreateNamedVotersPoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() 
                {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                var namedVotersPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = true,
                    ChoiceAdding = false,
                    ElectionMode = false
                };

                testContext.Polls.Add(namedVotersPoll);
                testContext.SaveChanges();

                return namedVotersPoll;
            }
        }

        [TestClass]
        public class ChoiceAddingTests : E2ETest
        {
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollVoteUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";

            [TestMethod]
            [TestCategory("E2E")]
            public void ChoiceAddingPoll_UserChoiceInputVisible()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateChoiceAddingPoll(context);

                        GoToUrl(driver, PollVoteUrl);

                        IWebElement addChoiceInput = FindElementById(driver, "user-choice-input");

                        Assert.IsTrue(addChoiceInput.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void NonChoiceAddingPoll_UserChoiceInputNotVisible()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreateChoiceAddingPoll(context);
                        poll.ChoiceAdding = false;
                        context.SaveChanges();

                        GoToUrl(driver, PollVoteUrl);

                        IWebElement addChoiceInput = FindElementById(driver, "user-choice-input");

                        Assert.IsFalse(addChoiceInput.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void Add_AddsChoice()
            {
                const string newChoiceName = "NEW CHOICE";

                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreateChoiceAddingPoll(context);

                        GoToUrl(driver, PollVoteUrl);

                        IWebElement addChoiceInput = FindElementById(driver, "user-choice-input");
                        addChoiceInput.SendKeys(newChoiceName);

                        IWebElement formButton = FindElementById(driver, "add-user-choice-button");
                        formButton.Click();

                        IReadOnlyCollection<IWebElement> choiceNames = driver.FindElements(NgBy.Binding("choice.Name"));

                        Assert.AreEqual(poll.Choices.Count + 1, choiceNames.Count);
                        Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                        // Refresh to ensure the new choice was stored in DB
                        GoToUrl(driver, PollVoteUrl);

                        IReadOnlyCollection<IWebElement> choiceNamesAfterRefresh = driver.FindElements(NgBy.Binding("choice.Name"));

                        Assert.AreEqual(poll.Choices.Count + 1, choiceNamesAfterRefresh.Count);
                        Assert.AreEqual(newChoiceName, choiceNamesAfterRefresh.Last().Text);
                    }
                }
            }

            public static Poll CreateChoiceAddingPoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() 
                {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                };

                // Open, Anonymous, Choice Adding, Shown Results
                var choiceAddingPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = true,
                    ElectionMode = false
                };

                testContext.Polls.Add(choiceAddingPointsPoll);
                testContext.SaveChanges();

                return choiceAddingPointsPoll;
            }
        }

        [TestClass]
        public class ElectionModeConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _electionModePointsPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                new Choice(){ Name = "Test Choice 2", 
                              Description = "A very long test description 2 that should exceed the character limit for descriptions" }};

                // Open, Anonymous, No Choice Adding, Shown Results
                _electionModePointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = true,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_electionModePointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_electionModePointsPoll);

                _context.Dispose();
            }

            [TestInitialize]
            public virtual void TestInitialise()
            {
                _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
                _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));
            }

            [TestCleanup]
            public void TestCleanUp()
            {
                _driver.Dispose();
            }

            [Ignore]
            [TestMethod]
            [TestCategory("E2E")]
            public void ElectionModePoll_DoesNotShowResultsButton()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement resultButton = _driver.FindElement(By.Id("results-button"));

                Assert.IsFalse(resultButton.IsVisible());
            }
        }
    }
}
