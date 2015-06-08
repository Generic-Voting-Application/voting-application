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
    public class MultiPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";

        [TestClass]
        public class DefaultPollConfiguration
        {
            private static ITestVotingContext _context;

            private const int truncatedTextLimit = 60;
            private static readonly int WaitTime = 500;
            private static Poll _defaultMultiPoll;
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
                _defaultMultiPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    IsElectionMode = false
                };

                _context.Polls.Add(_defaultMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_defaultMultiPoll);

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
                _driver.Navigate().GoToUrl(PollUrl);
                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_defaultMultiPoll.Choices.Count, choiceNames.Count);
                CollectionAssert.AreEquivalent(_defaultMultiPoll.Choices.Select(o => o.Name).ToList(),
                                               choiceNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PopulatedChoices_DisplaysChoiceDescriptions()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IReadOnlyCollection<IWebElement> choiceDescriptions = _driver.FindElements(NgBy.Model("choice.Description"));

                Assert.AreEqual(_defaultMultiPoll.Choices.Count, choiceDescriptions.Count);

                CollectionAssert.AreEquivalent(_defaultMultiPoll.Choices.Select(o => o.Description).ToList(),
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
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _defaultMultiPoll.UUID));
            }

            [TestMethod, TestCategory("E2E")]
            public void DefaultPoll_ShowsResultsButton()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement resultsLink = _driver.FindElement(By.Id("results-button"));

                Assert.IsTrue(resultsLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void MultiVote_AfterVoting_VoteIsRemembered()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                choices.First().Click();
                voteButton.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                IWebElement selectedChoice = _driver.FindElements(By.CssSelector(".selected-choice")).Single();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(selectedChoice.IsVisible());
            }
        }

        [TestClass]
        public class InviteOnlyPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _inviteOnlyMultiPoll;
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
                _inviteOnlyMultiPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Multi,
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

                _context.Polls.Add(_inviteOnlyMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_inviteOnlyMultiPoll);

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
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement error = _driver.FindElement(NgBy.Binding("$root.error.readableMessage"));

                Assert.IsTrue(error.IsVisible());
                Assert.AreEqual("This poll is invite only", error.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void AccessWithToken_DisplaysAllChoices()
            {
                _driver.Navigate().GoToUrl(PollUrl + "/" + _inviteOnlyMultiPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_inviteOnlyMultiPoll.Choices.Count, choiceNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyMultiPoll.Choices.Select(o => o.Name).ToList(),
                                               choiceNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void VoteWithToken_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(PollUrl + "/" + _inviteOnlyMultiPoll.Ballots[0].TokenGuid);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _inviteOnlyMultiPoll.UUID));
            }
        }

        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _namedMultiPoll;
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
                _namedMultiPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = true,
                    ChoiceAdding = false,
                    IsElectionMode = false
                };

                _context.Polls.Add(_namedMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_namedMultiPoll);

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
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                Assert.AreEqual(PollUrl, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void NameInput_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(PollUrl);
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
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();


                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                IWebElement goButton = _driver.FindElement(By.Id("go-button"));
                formName.SendKeys("User");

                IWebElement form = _driver.FindElement(By.Name("loginForm"));
                form.Submit();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _namedMultiPoll.UUID));
            }
        }

        [TestClass]
        public class ChoiceAddingPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _choiceAddingMultiPoll;
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
                _choiceAddingMultiPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = true,
                    IsElectionMode = false
                };

                _context.Polls.Add(_choiceAddingMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_choiceAddingMultiPoll);

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
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));

                Assert.IsTrue(addChoiceLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void ChoiceAddingLink_PromptsForChoiceDetails()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));
                addChoiceLink.Click();

                Assert.AreEqual(PollUrl, _driver.Url);

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
                _driver.Navigate().GoToUrl(PollUrl);
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
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement addChoiceLink = _driver.FindElement(By.Id("add-choice-link"));
                addChoiceLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
                IWebElement addButton = _driver.FindElement(By.Id("add-button"));

                String newChoiceName = "New Choice";
                formName.SendKeys(newChoiceName);

                IWebElement formButton = _driver.FindElement(By.Id("add-button"));
                formButton.Click();

                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_choiceAddingMultiPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                // Refresh to ensure the new choice was stored in DB
                _driver.Navigate().GoToUrl(PollUrl);

                choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_choiceAddingMultiPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                _choiceAddingMultiPoll.Choices.Remove(_choiceAddingMultiPoll.Choices.Last());
                _context.SaveChanges();
            }
        }

        [TestClass]
        public class IsElectionModeConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _isElectionModeMultiPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Choice> testPollChoices = new List<Choice>() {
                    new Choice()
                    { 
                        Name = "Test Choice 1", Description = "Test Description 1" 
                    }
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                _isElectionModeMultiPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    IsElectionMode = true
                };

                _context.Polls.Add(_isElectionModeMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_isElectionModeMultiPoll);

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
