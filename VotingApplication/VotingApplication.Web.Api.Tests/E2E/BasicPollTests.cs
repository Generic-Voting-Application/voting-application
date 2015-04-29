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
    public class BasicPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";

        [TestClass]
        public class DefaultPollConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private const int truncatedTextLimit = 60;
            private static Poll _defaultBasicPoll;

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
                _defaultBasicPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = false
                };

                _context.Polls.Add(_defaultBasicPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_defaultBasicPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void PopulatedOptions_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultBasicPoll.UUID);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_defaultBasicPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_defaultBasicPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod]
            public void PopulatedOptions_DisplaysAllOptionDescriptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultBasicPoll.UUID);
                IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Model("option.Description"));

                Assert.AreEqual(_defaultBasicPoll.Options.Count, optionDescriptions.Count);
                List<String> expectedDescriptions = new List<string>();

                foreach (Option option in _defaultBasicPoll.Options)
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
            public void VotingOnOption_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultBasicPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _defaultBasicPoll.UUID));
            }
        }

        [TestClass]
        public class InviteOnlyPollConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private static Poll _inviteOnlyBasicPoll;

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
                _inviteOnlyBasicPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
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

                _context.Polls.Add(_inviteOnlyBasicPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_inviteOnlyBasicPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void AccessWithNoToken_DisplaysError()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyBasicPoll.UUID);
                IWebElement error = _driver.FindElement(NgBy.Binding("$root.error.readableMessage"));

                Assert.IsTrue(error.IsVisible());
                Assert.AreEqual("This poll is invite only", error.Text);
            }

            [TestMethod]
            public void AccessWithToken_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyBasicPoll.UUID + "/" + _inviteOnlyBasicPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_inviteOnlyBasicPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyBasicPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod]
            public void VoteWithToken_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyBasicPoll.UUID + "/" + _inviteOnlyBasicPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _inviteOnlyBasicPoll.UUID));
            }
        }

        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static IWebDriver _driver;
            private static IVotingContext _context;

            private static Poll _namedBasicPoll;

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
                _namedBasicPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = true,
                    OptionAdding = false,
                    HiddenResults = false
                };

                _context.Polls.Add(_namedBasicPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_namedBasicPoll);

                _driver.Dispose();
                _context.Dispose();
            }

            [TestMethod]
            public void VoteWithNoName_PromptsForName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedBasicPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _namedBasicPoll.UUID, _driver.Url);

                IWebElement form = _driver.FindElement(NgBy.Model("loginForm.name"));
                Assert.IsTrue(form.IsVisible());
                Assert.AreEqual(String.Empty, form.Text);
            }

            [TestMethod]
            public void NameInput_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedBasicPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                buttons = _driver.FindElements(By.TagName("Button"));

                IWebElement form = _driver.FindElement(NgBy.Model("loginForm.name"));
                IWebElement goButton = buttons.First(b => b.Text == "Go");

                Assert.IsTrue(goButton.IsVisible());
                Assert.IsFalse(goButton.Enabled);

                form.SendKeys("User");

                Assert.IsTrue(goButton.Enabled);
            }

            [TestMethod]
            public void NameInput_VotesUponSubmission()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedBasicPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                buttons = _driver.FindElements(By.TagName("Button"));

                IWebElement form = _driver.FindElement(NgBy.Model("loginForm.name"));
                IWebElement goButton = buttons.First(b => b.Text == "Go");
                form.SendKeys("User");
                form.Submit();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _namedBasicPoll.UUID));
            }
        }
    }
}
