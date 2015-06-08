using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Tests.E2E.Helpers;
using VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers;

namespace VotingApplication.Web.Tests.E2E
{
    public class UpDownPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
        private static readonly int WaitTime = 500;

        [TestClass]
        public class DefaultPollConfiguration
        {
            private static ITestVotingContext _context;

            private const int truncatedTextLimit = 60;
            private static Poll _defaultUpDownPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" }
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                _defaultUpDownPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    IsElectionMode = false
                };

                _context.Polls.Add(_defaultUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_defaultUpDownPoll);

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
            public void PopulatedChoices_DisplaysAllChoices()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_defaultUpDownPoll.Choices.Count, choiceNames.Count);
                CollectionAssert.AreEquivalent(_defaultUpDownPoll.Choices.Select(o => o.Name).ToList(),
                                               choiceNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PopulatedChoices_DisplaysChoiceDescriptions()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IReadOnlyCollection<IWebElement> choiceDescriptions = _driver.FindElements(NgBy.Model("choice.Description"));

                Assert.AreEqual(_defaultUpDownPoll.Choices.Count, choiceDescriptions.Count);

                CollectionAssert.AreEquivalent(_defaultUpDownPoll.Choices.Select(o => o.Description).ToList(),
                                               choiceDescriptions.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PopulatedChoices_TruncatesLongDescriptions()
            {
                string truncatedString = new String('a', truncatedTextLimit / 2);
                _driver.Navigate().GoToUrl(PollUrl);

                Poll poll = _context.Polls.Where(p => p.UUID == PollGuid).Single();
                poll.Choices.Add(new Choice()
                {
                    Name = "Test Choice 2",
                    Description = truncatedString + " " + truncatedString
                });

                _context.SaveChanges();

                Thread.Sleep(WaitTime);

                IReadOnlyCollection<IWebElement> choiceDescriptions = _driver.FindElements(NgBy.Model("choice.Description"));

                Assert.AreEqual(truncatedString + "... Show More", choiceDescriptions.Select(o => o.Text).Last());

                poll.Choices.Remove(poll.Choices.Last());
                _context.SaveChanges();
            }

            [TestMethod, TestCategory("E2E")]
            public void VotingOnChoice_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                IReadOnlyCollection<IWebElement> firstChoiceButtons = choices.First().FindElements(By.TagName("Button"));
                firstChoiceButtons.First().Click();

                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _defaultUpDownPoll.UUID));
            }

            [TestMethod, TestCategory("E2E")]
            public void DefaultPoll_ShowsResultsButton()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement resultsLink = _driver.FindElement(By.Id("results-button"));

                Assert.IsTrue(resultsLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void AfterVoting_VoteIsRemembered()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                IReadOnlyCollection<IWebElement> firstChoiceButtons = choices.First().FindElements(By.TagName("Button"));
                firstChoiceButtons.First().Click();

                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));
                IWebElement selectedChoiceButton = choices.First().FindElements(By.CssSelector(".active-btn")).Single();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(selectedChoiceButton.IsVisible());
            }
        }

        [TestClass]
        public class InviteOnlyPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _inviteOnlyUpDownPoll;
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
                _inviteOnlyUpDownPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.UpDown,
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
                    }
                };

                _context.Polls.Add(_inviteOnlyUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_inviteOnlyUpDownPoll);

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
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyUpDownPoll.UUID);
                IWebElement error = _driver.FindElement(NgBy.Binding("$root.error.readableMessage"));

                Assert.IsTrue(error.IsVisible());
                Assert.AreEqual("This poll is invite only", error.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void AccessWithToken_DisplaysAllChoices()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyUpDownPoll.UUID + "/" + _inviteOnlyUpDownPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_inviteOnlyUpDownPoll.Choices.Count, choiceNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyUpDownPoll.Choices.Select(o => o.Name).ToList(),
                                               choiceNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void VoteWithToken_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyUpDownPoll.UUID + "/" + _inviteOnlyUpDownPoll.Ballots[0].TokenGuid);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _inviteOnlyUpDownPoll.UUID));
            }
        }

        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _namedUpDownPoll;
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
                _namedUpDownPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = true,
                    ChoiceAdding = false,
                    IsElectionMode = false
                };

                _context.Polls.Add(_namedUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_namedUpDownPoll);

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
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void NameInput_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

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
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID);
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

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _namedUpDownPoll.UUID));
            }
        }

        [TestClass]
        public class ChoiceAddingPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _choiceAddingUpDownPoll;
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
                new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                new Choice(){ Name = "Test Choice 2", Description = "Test Description 2" }};

                // Open, Named voters, No Choice Adding, Shown Results
                _choiceAddingUpDownPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = true,
                    IsElectionMode = false
                };

                _context.Polls.Add(_choiceAddingUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_choiceAddingUpDownPoll);

                _context.Dispose();
            }

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingPoll_ProvidesLinkForAddingChoices()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingUpDownPoll.UUID);

                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));

                Assert.IsTrue(addChoiceLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingLink_PromptsForChoiceDetails()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingUpDownPoll.UUID);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));
                addChoiceLink.Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingUpDownPoll.UUID, _driver.Url);

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
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingUpDownPoll.UUID);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));
                addChoiceLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
                IWebElement addButton = _driver.FindElement(By.Id("add-button"));

                Assert.IsTrue(addButton.IsVisible());
                Assert.IsFalse(addButton.Enabled);

                formName.SendKeys("New Choice");

                Assert.IsTrue(addButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingSubmission_AddsChoice()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingUpDownPoll.UUID);
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

                Assert.AreEqual(_choiceAddingUpDownPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                // Refresh to ensure they new choice was stored in DB
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _choiceAddingUpDownPoll.UUID);

                choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_choiceAddingUpDownPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);
            }
        }

        [TestClass]
        public class IsElectionModeConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _isElectionModeUpDownPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" }
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                _isElectionModeUpDownPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    IsElectionMode = true
                };

                _context.Polls.Add(_isElectionModeUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_isElectionModeUpDownPoll);

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
