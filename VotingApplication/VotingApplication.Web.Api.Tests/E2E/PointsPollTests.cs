using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Tests.E2E.Helpers;
using VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers;

namespace VotingApplication.Web.Tests.E2E
{
    public class PointsPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";

        [TestClass]
        public class DefaultPollConfiguration
        {
            private static ITestVotingContext _context;
            private const int truncatedTextLimit = 60;
            private static Poll _defaultPointsPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

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

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" }
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                _defaultPointsPoll = new Poll()
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
                    IsElectionMode = false,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_defaultPointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_defaultPointsPoll);

                _context.Dispose();
            }

            [TestMethod, TestCategory("E2E")]
            public void PopulatedChoices_DisplaysAllChoices()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_defaultPointsPoll.Choices.Count, choiceNames.Count);
                CollectionAssert.AreEquivalent(_defaultPointsPoll.Choices.Select(o => o.Name).ToList(),
                                               choiceNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PopulatedChoices_DisplaysAllChoiceDescriptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> choiceDescriptions = _driver.FindElements(NgBy.Model("choice.Description"));

                Assert.AreEqual(_defaultPointsPoll.Choices.Count, choiceDescriptions.Count);
                List<String> expectedDescriptions = new List<string>();

                foreach (Choice choice in _defaultPointsPoll.Choices)
                {
                    string truncatedDescription = choice.Description
                                                          .Split(' ')
                                                          .Aggregate((prev, curr) => (curr.Length + prev.Length >= truncatedTextLimit) ?
                                                                                     prev : prev + " " + curr);

                    if (truncatedDescription != choice.Description)
                    {
                        truncatedDescription += "... Show More";
                    }
                    expectedDescriptions.Add(truncatedDescription);
                }

                CollectionAssert.AreEquivalent(expectedDescriptions,
                                               choiceDescriptions.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void VotingOnChoice_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                IReadOnlyCollection<IWebElement> increaseChoiceButtons = _driver.FindElements(By.Id("increase-button"));
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));

                increaseChoiceButtons.First().Click();
                voteButton.Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _defaultPointsPoll.UUID));
            }

            [TestMethod, TestCategory("E2E")]
            public void DefaultPoll_ShowsResultsButton()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IWebElement resultsLink = _driver.FindElement(By.Id("results-button"));

                Assert.IsTrue(resultsLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsButtons_ChangePointAllocations()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                IReadOnlyCollection<IWebElement> firstChoiceButtons = choices.First().FindElements(By.TagName("Button"));
                IWebElement selectedChoicePoints = choices.First().FindElement(NgBy.Binding("choice.voteValue"));
                IReadOnlyCollection<IWebElement> increaseChoiceButtons = _driver.FindElements(By.Id("increase-button"));
                IReadOnlyCollection<IWebElement> decreaseChoiceButtons = _driver.FindElements(By.Id("decrease-button"));

                IWebElement plusButton = increaseChoiceButtons.First();
                IWebElement minusButton = decreaseChoiceButtons.First();

                Assert.IsTrue(selectedChoicePoints.IsVisible());
                Assert.AreEqual("0/" + _defaultPointsPoll.MaxPerVote, selectedChoicePoints.Text);

                plusButton.Click();

                Assert.AreEqual("1/" + _defaultPointsPoll.MaxPerVote, selectedChoicePoints.Text);

                minusButton.Click();

                Assert.AreEqual("0/" + _defaultPointsPoll.MaxPerVote, selectedChoicePoints.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsButtons_ChangeTotalPointLimit()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                IReadOnlyCollection<IWebElement> firstChoiceButtons = choices.First().FindElements(By.TagName("Button"));
                IWebElement totalPoints = _driver.FindElement(By.Id("points-display"));

                IReadOnlyCollection<IWebElement> increaseChoiceButtons = _driver.FindElements(By.Id("increase-button"));
                IReadOnlyCollection<IWebElement> decreaseChoiceButtons = _driver.FindElements(By.Id("decrease-button"));

                IWebElement plusButton = increaseChoiceButtons.First();
                IWebElement minusButton = decreaseChoiceButtons.First();

                Assert.IsTrue(totalPoints.IsVisible());
                Assert.AreEqual("Unallocated points: " + _defaultPointsPoll.MaxPoints + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);

                plusButton.Click();

                Assert.AreEqual("Unallocated points: " + (_defaultPointsPoll.MaxPoints - 1) + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);

                minusButton.Click();

                Assert.AreEqual("Unallocated points: " + _defaultPointsPoll.MaxPoints + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPerChoice_CanNotExceedMaximum()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                IReadOnlyCollection<IWebElement> increaseChoiceButtons = _driver.FindElements(By.Id("increase-button"));

                IWebElement plusButton = increaseChoiceButtons.First();

                for (int i = 0; i < _defaultPointsPoll.MaxPerVote; i++)
                {
                    plusButton.Click();
                }

                Assert.IsFalse(plusButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void TotalPoints_CanNotExceedMaximum()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                int allocatedPoints = 0;

                foreach (IWebElement choice in choices)
                {
                    IReadOnlyCollection<IWebElement> increaseChoiceButtons = _driver.FindElements(By.Id("increase-button"));

                    IWebElement plusButton = increaseChoiceButtons.First();

                    for (int i = 0; i < _defaultPointsPoll.MaxPerVote; i++)
                    {
                        if (allocatedPoints < _defaultPointsPoll.MaxPoints)
                        {
                            plusButton.Click();
                            allocatedPoints++;
                        }
                    }
                }

                foreach (IWebElement choice in choices)
                {
                    IWebElement increaseButton = choice.FindElement(By.Id("increase-button"));

                    Assert.IsFalse(increaseButton.Enabled);
                }
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsVote_AfterVoting_VoteIsRemembered()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                IReadOnlyCollection<IWebElement> increaseChoiceButtons = _driver.FindElements(By.Id("increase-button"));

                IWebElement plusButton = increaseChoiceButtons.First();
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));

                plusButton.Click();
                voteButton.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));
                IWebElement selectedChoicePoints = choices.First().FindElement(NgBy.Binding("poll.MaxPerVote"));

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(selectedChoicePoints.IsVisible());
                Assert.AreEqual("1/" + _defaultPointsPoll.MaxPerVote, selectedChoicePoints.Text);
            }
        }

        [TestClass]
        public class InviteOnlyPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _inviteOnlyPointsPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                new Choice(){ Name = "Test Choice 2", Description = "Test Description 2" }};

                // Invite Only, Anonymous, No Choice Adding, Shown Results
                _inviteOnlyPointsPoll = new Poll()
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
                    IsElectionMode = false,
                    Ballots = new List<Ballot>()
                    {
                        new Ballot() { TokenGuid = Guid.NewGuid() }
                    },
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_inviteOnlyPointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_inviteOnlyPointsPoll);

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

            [TestMethod, TestCategory("E2E")]
            public void AccessWithNoToken_DisplaysError()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyPointsPoll.UUID);
                IWebElement error = _driver.FindElement(NgBy.Binding("$root.error.readableMessage"));

                Assert.IsTrue(error.IsVisible());
                Assert.AreEqual("This poll is invite only", error.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void AccessWithToken_DisplaysAllChoices()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyPointsPoll.UUID + "/" + _inviteOnlyPointsPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_inviteOnlyPointsPoll.Choices.Count, choiceNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyPointsPoll.Choices.Select(o => o.Name).ToList(),
                                               choiceNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void VoteWithToken_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyPointsPoll.UUID + "/" + _inviteOnlyPointsPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _inviteOnlyPointsPoll.UUID));
            }
        }

        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _namedPointsPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                new Choice(){ Name = "Test Choice 2", Description = "Test Description 2" }};

                // Open, Named voters, No Choice Adding, Shown Results
                _namedPointsPoll = new Poll()
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
                    IsElectionMode = false,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_namedPointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_namedPointsPoll);

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

            [TestMethod, TestCategory("E2E")]
            public void VoteWithNoName_PromptsForName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void NameInput_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                buttons = _driver.FindElements(By.TagName("Button"));

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                IWebElement goButton = _driver.FindElement(By.Id("go-button"));

                Assert.IsTrue(goButton.IsVisible());
                Assert.IsFalse(goButton.Enabled);

                formName.SendKeys("User");

                Assert.IsTrue(goButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void NameInput_VotesUponSubmission()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                buttons = _driver.FindElements(By.TagName("Button"));

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                IWebElement goButton = _driver.FindElement(By.Id("go-button"));
                formName.SendKeys("User");

                IWebElement form = _driver.FindElement(By.Name("loginForm"));
                form.Submit();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _namedPointsPoll.UUID));
            }
        }

        [TestClass]
        public class ChoiceAddingPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _choiceAddingPointsPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                new Choice(){ Name = "Test Choice 2", Description = "Test Description 2" }};

                // Open, Named voters, No Choice Adding, Shown Results
                _choiceAddingPointsPoll = new Poll()
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
                    IsElectionMode = false,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_choiceAddingPointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_choiceAddingPointsPoll);

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

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingPoll_ProvidesLinkForAddingChoices()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingPointsPoll.UUID);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));

                Assert.IsTrue(addChoiceLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingLink_PromptsForChoiceDetails()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingPointsPoll.UUID);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));
                addChoiceLink.Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingPointsPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);

                IWebElement formDescription = _driver.FindElement(NgBy.Model("addChoiceForm.description"));
                Assert.IsTrue(formDescription.IsVisible());
                Assert.AreEqual(String.Empty, formDescription.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingPrompt_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingPointsPoll.UUID);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));
                addChoiceLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));

                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement addButton = _driver.FindElement(By.Id("add-button"));

                Assert.IsTrue(addButton.IsVisible());
                Assert.IsFalse(addButton.Enabled);

                formName.SendKeys("New Choice");

                Assert.IsTrue(addButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingSubmission_AddsChoice()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingPointsPoll.UUID);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));
                addChoiceLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));

                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement addButton = _driver.FindElement(By.Id("add-button"));

                String newChoiceName = "New Choice";
                formName.SendKeys(newChoiceName);

                IWebElement formButton = _driver.FindElement(By.Id("add-button"));
                formButton.Click();

                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_choiceAddingPointsPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                // Refresh to ensure they new choice was stored in DB
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingPointsPoll.UUID);

                choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_choiceAddingPointsPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);
            }
        }

        [TestClass]
        public class IsElectionModeConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _isElectionModePointsPoll;
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
                _isElectionModePointsPoll = new Poll()
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
                    IsElectionMode = true,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_isElectionModePointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_isElectionModePointsPoll);

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

            [TestMethod, TestCategory("E2E")]
            public void IsElectionModePoll_DoesNotShowResultsButton()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement resultButton = _driver.FindElement(By.Id("results-button"));

                Assert.IsFalse(resultButton.IsVisible());
            }
        }
    }
}
