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
    public class MultiPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";

        [TestClass]
        public class DefaultPollConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private const int truncatedTextLimit = 60;
            private static Poll _defaultMultiPoll;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
                _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));

                ContextFactory contextFactory = new ContextFactory();
                _context = contextFactory.CreateTestContext();

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", 
                              Description = "A very long test description 2 that should exceed the character limit for descriptions" }};

                // Open, Anonymous, No Option Adding, Shown Results
                _defaultMultiPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = false
                };

                _context.Polls.Add(_defaultMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_defaultMultiPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void MultiPoll_PopulatedOptions_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_defaultMultiPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_defaultMultiPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod]
            public void MultiPoll_PopulatedOptions_DisplaysAllOptionDescriptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Model("option.Description"));

                Assert.AreEqual(_defaultMultiPoll.Options.Count, optionDescriptions.Count);
                List<String> expectedDescriptions = new List<string>();

                foreach (Option option in _defaultMultiPoll.Options)
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

            [TestMethod]
            public void MultiPoll_VotingOnOption_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _defaultMultiPoll.UUID));
            }

            [TestMethod]
            public void MultiPoll_DefaultPoll_ShowsResultsButton()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement resultsLink = anchors.FirstOrDefault(a => a.Text == "Go to results");

                Assert.IsTrue(resultsLink.IsVisible());
            }
        }

        [TestClass]
        public class InviteOnlyPollConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private static Poll _inviteOnlyMultiPoll;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
                _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));

                ContextFactory contextFactory = new ContextFactory();
                _context = contextFactory.CreateTestContext();

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Invite Only, Anonymous, No Option Adding, Shown Results
                _inviteOnlyMultiPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.Multi,
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

                _context.Polls.Add(_inviteOnlyMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_inviteOnlyMultiPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void MultiPoll_AccessWithNoToken_DisplaysError()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyMultiPoll.UUID);
                IWebElement error = _driver.FindElement(NgBy.Binding("$root.error.readableMessage"));

                Assert.IsTrue(error.IsVisible());
                Assert.AreEqual("This poll is invite only", error.Text);
            }

            [TestMethod]
            public void MultiPoll_AccessWithToken_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyMultiPoll.UUID + "/" + _inviteOnlyMultiPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_inviteOnlyMultiPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyMultiPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod]
            public void MultiPoll_VoteWithToken_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyMultiPoll.UUID + "/" + _inviteOnlyMultiPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _inviteOnlyMultiPoll.UUID));
            }
        }

        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private static Poll _namedMultiPoll;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
                _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));

                ContextFactory contextFactory = new ContextFactory();
                _context = contextFactory.CreateTestContext();

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Open, Named voters, No Option Adding, Shown Results
                _namedMultiPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = true,
                    OptionAdding = false,
                    HiddenResults = false
                };

                _context.Polls.Add(_namedMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_namedMultiPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void MultiPoll_VoteWithNoName_PromptsForName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _namedMultiPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);
            }

            [TestMethod]
            public void MultiPoll_NameInput_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedMultiPoll.UUID);
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

            [TestMethod]
            public void MultiPoll_NameInput_VotesUponSubmission()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedMultiPoll.UUID);
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

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _namedMultiPoll.UUID));
            }
        }

        [TestClass]
        public class OptionAddingPollConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private static Poll _optionAddingMultiPoll;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
                _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));

                ContextFactory contextFactory = new ContextFactory();
                _context = contextFactory.CreateTestContext();

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Open, Named voters, No Option Adding, Shown Results
                _optionAddingMultiPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = true,
                    HiddenResults = false
                };

                _context.Polls.Add(_optionAddingMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_optionAddingMultiPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void MultiPoll_OptionAddingPoll_ProvidesLinkForAddingOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");

                Assert.IsTrue(addOptionLink.IsVisible());
            }

            [TestMethod]
            public void MultiPoll_OptionAddingLink_PromptsForOptionDetails()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");
                addOptionLink.Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _optionAddingMultiPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);

                IWebElement formDescription = _driver.FindElement(NgBy.Model("addOptionForm.description"));
                Assert.IsTrue(formDescription.IsVisible());
                Assert.AreEqual(String.Empty, formDescription.Text);
            }

            [TestMethod]
            public void MultiPoll_OptionAddingPrompt_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingMultiPoll.UUID);
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

            [TestMethod]
            public void MultiPoll_OptionAddingSubmission_AddsOption()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingMultiPoll.UUID);
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

                Assert.AreEqual(_optionAddingMultiPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);

                // Refresh to ensure they new option was stored in DB
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingMultiPoll.UUID);

                optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingMultiPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);
            }
        }

        [TestClass]
        public class HiddenResultsConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private static Poll _hiddenResultsMultiPoll;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
                _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));

                ContextFactory contextFactory = new ContextFactory();
                _context = contextFactory.CreateTestContext();

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", 
                              Description = "A very long test description 2 that should exceed the character limit for descriptions" }};

                // Open, Anonymous, No Option Adding, Shown Results
                _hiddenResultsMultiPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    PollType = PollType.Multi,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = true
                };

                _context.Polls.Add(_hiddenResultsMultiPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_hiddenResultsMultiPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void MultiPoll_HiddenResultsPoll_DoesNotShowResultsButton()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _hiddenResultsMultiPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement resultsLink = anchors.FirstOrDefault(a => a.Text == "Go to results");

                Assert.IsNull(resultsLink);
            }
        }
    }
}
