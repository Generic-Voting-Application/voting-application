using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Tests.E2E.Helpers;
using VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers;

namespace VotingApplication.Web.Tests.E2E
{
    public class UpDownPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";

        #region Default Config
        [TestClass]
        public class DefaultPollConfiguration
        {
            private static IVotingContext _context;

            private const int truncatedTextLimit = 60;
            private static Poll _defaultUpDownPoll;

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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", 
                              Description = "A very long test description 2 that should exceed the character limit for descriptions" }};

                // Open, Anonymous, No Option Adding, Shown Results
                _defaultUpDownPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = false
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

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_PopulatedOptions_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_defaultUpDownPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_defaultUpDownPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_PopulatedOptions_DisplaysAllOptionDescriptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Model("option.Description"));

                Assert.AreEqual(_defaultUpDownPoll.Options.Count, optionDescriptions.Count);
                List<String> expectedDescriptions = new List<string>();

                foreach (Option option in _defaultUpDownPoll.Options)
                {
                    string truncatedDescription = option.Description
                                                          .Split(' ')
                                                          .Aggregate((prev, curr) => (curr.Length + prev.Length >= truncatedTextLimit) ?
                                                                                     prev : prev + " " + curr);

                    if (truncatedDescription != option.Description)
                    {
                        truncatedDescription += "... Show More";
                    }
                    expectedDescriptions.Add(truncatedDescription);
                }

                CollectionAssert.AreEquivalent(expectedDescriptions,
                                               optionDescriptions.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_VotingOnOption_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                firstOptionButtons.First().Click();
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _defaultUpDownPoll.UUID));
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_DefaultPoll_ShowsResultsButton()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement resultsLink = anchors.FirstOrDefault(a => a.Text == "Go to results");

                Assert.IsTrue(resultsLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownVote_AfterVoting_VoteIsRemembered()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                firstOptionButtons.First().Click();
                buttons.First(b => b.Text == "Vote").Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));
                IWebElement selectedOptionButton = options.First().FindElements(By.CssSelector(".active-btn")).Single();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(selectedOptionButton.IsVisible());
            }
        }
        #endregion

        #region Invite Only Config
        [TestClass]
        public class InviteOnlyPollConfiguration
        {
            private static IVotingContext _context;

            private static Poll _inviteOnlyUpDownPoll;

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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Invite Only, Anonymous, No Option Adding, Shown Results
                _inviteOnlyUpDownPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = true,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = false,
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

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_AccessWithNoToken_DisplaysError()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyUpDownPoll.UUID);
                IWebElement error = _driver.FindElement(NgBy.Binding("$root.error.readableMessage"));

                Assert.IsTrue(error.IsVisible());
                Assert.AreEqual("This poll is invite only", error.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_AccessWithToken_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyUpDownPoll.UUID + "/" + _inviteOnlyUpDownPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_inviteOnlyUpDownPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyUpDownPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_VoteWithToken_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyUpDownPoll.UUID + "/" + _inviteOnlyUpDownPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _inviteOnlyUpDownPoll.UUID));
            }
        }
        #endregion

        #region Named Voters Config
        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static IVotingContext _context;

            private static Poll _namedUpDownPoll;

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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Open, Named voters, No Option Adding, Shown Results
                _namedUpDownPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = true,
                    OptionAdding = false,
                    HiddenResults = false
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

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_VoteWithNoName_PromptsForName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_NameInput_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                buttons = _driver.FindElements(By.TagName("Button"));

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                IWebElement goButton = buttons.First(b => b.Text == "Go");

                Assert.IsTrue(goButton.IsVisible());
                Assert.IsFalse(goButton.Enabled);

                formName.SendKeys("User");

                Assert.IsTrue(goButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_NameInput_VotesUponSubmission()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                buttons = _driver.FindElements(By.TagName("Button"));

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                IWebElement goButton = buttons.First(b => b.Text == "Go");
                formName.SendKeys("User");

                IWebElement form = _driver.FindElement(By.Name("loginForm"));
                form.Submit();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _namedUpDownPoll.UUID));
            }
        }
        #endregion

        #region Option Adding Config
        [TestClass]
        public class OptionAddingPollConfiguration
        {
            private static IVotingContext _context;

            private static Poll _optionAddingUpDownPoll;

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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Open, Named voters, No Option Adding, Shown Results
                _optionAddingUpDownPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = true,
                    HiddenResults = false
                };

                _context.Polls.Add(_optionAddingUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_optionAddingUpDownPoll);

                _context.Dispose();
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_OptionAddingPoll_ProvidesLinkForAddingOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");

                Assert.IsTrue(addOptionLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_OptionAddingLink_PromptsForOptionDetails()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");
                addOptionLink.Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _optionAddingUpDownPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);

                IWebElement formDescription = _driver.FindElement(NgBy.Model("addOptionForm.description"));
                Assert.IsTrue(formDescription.IsVisible());
                Assert.AreEqual(String.Empty, formDescription.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_OptionAddingPrompt_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");
                addOptionLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement doneButton = buttons.First(b => b.Text == "Done");

                Assert.IsTrue(doneButton.IsVisible());
                Assert.IsFalse(doneButton.Enabled);

                formName.SendKeys("New Option");

                Assert.IsTrue(doneButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_OptionAddingSubmission_AddsOption()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");
                addOptionLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement doneButton = buttons.First(b => b.Text == "Done");

                String newOptionName = "New Option";
                formName.SendKeys(newOptionName);

                IWebElement form = _driver.FindElement(By.Name("addOptionForm"));
                form.Submit();

                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingUpDownPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);

                // Refresh to ensure they new option was stored in DB
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingUpDownPoll.UUID);

                optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingUpDownPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);
            }
        }
        #endregion

        #region Hidden Results Config
        [TestClass]
        public class HiddenResultsConfiguration
        {
            private static IVotingContext _context;

            private static Poll _hiddenResultsUpDownPoll;

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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", 
                              Description = "A very long test description 2 that should exceed the character limit for descriptions" }};

                // Open, Anonymous, No Option Adding, Shown Results
                _hiddenResultsUpDownPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.UpDown,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = true
                };

                _context.Polls.Add(_hiddenResultsUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_hiddenResultsUpDownPoll);

                _context.Dispose();
            }

            [TestMethod, TestCategory("E2E")]
            public void UpDownPoll_HiddenResultsPoll_DoesNotShowResultsButton()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _hiddenResultsUpDownPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement resultsLink = anchors.FirstOrDefault(a => a.Text == "Go to results");

                Assert.IsNull(resultsLink);
            }
        }
        #endregion
    }
}
