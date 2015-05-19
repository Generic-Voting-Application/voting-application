using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            private static ITestVotingContext _context;

            private const int truncatedTextLimit = 60;
            private static Poll _defaultMultiPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Option> testPollOptions = new List<Option>() {
                    new Option(){ Name = "Test Option 1", Description = "Test Description 1" }
                };

                // Open, Anonymous, No Option Adding, Shown Results
                _defaultMultiPoll = new Poll()
                {
                    UUID = PollGuid,
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
            public void PopulatedOptions_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_defaultMultiPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_defaultMultiPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PopulatedOptions_DisplaysOptionDescriptions()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Model("option.Description"));

                Assert.AreEqual(_defaultMultiPoll.Options.Count, optionDescriptions.Count);

                CollectionAssert.AreEquivalent(_defaultMultiPoll.Options.Select(o => o.Description).ToList(),
                                               optionDescriptions.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public async Task PopulatedOptions_TruncatesLongDescriptions()
            {
                string truncatedString = new String('a', truncatedTextLimit / 2);
                _driver.Navigate().GoToUrl(PollUrl);

                Poll poll = _context.Polls.Where(p => p.UUID == PollGuid).Single();
                poll.Options.Add(new Option()
                {
                    Name = "Test Option 2",
                    Description = truncatedString + " " + truncatedString
                });

                await _context.SaveChangesAsync();

                IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Model("option.Description"));

                Assert.AreEqual(truncatedString + "... Show More", optionDescriptions.Select(o => o.Text).Last());

                poll.Options.Remove(poll.Options.Last());
                _context.SaveChanges();
            }

            [TestMethod, TestCategory("E2E")]
            public void VotingOnOption_NavigatesToResultsPage()
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
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                options.First().Click();
                voteButton.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                IWebElement selectedOption = _driver.FindElements(By.CssSelector(".selected-option")).Single();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(selectedOption.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void ClearVote_RemovesVote()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                options.First().Click();
                voteButton.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                IWebElement clearVoteOption = _driver.FindElements(By.Id("clear-vote-button")).Single();
                clearVoteOption.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                IWebElement selectedOption = _driver.FindElements(By.CssSelector(".selected-option")).FirstOrDefault();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsFalse(selectedOption.IsVisible());
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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Invite Only, Anonymous, No Option Adding, Shown Results
                _inviteOnlyMultiPoll = new Poll()
                {
                    UUID = PollGuid,
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
            public void AccessWithToken_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(PollUrl + "/" + _inviteOnlyMultiPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_inviteOnlyMultiPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyMultiPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Open, Named voters, No Option Adding, Shown Results
                _namedMultiPoll = new Poll()
                {
                    UUID = PollGuid,
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
        public class OptionAddingPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _optionAddingMultiPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Open, Named voters, No Option Adding, Shown Results
                _optionAddingMultiPoll = new Poll()
                {
                    UUID = PollGuid,
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
            public void OptionAddingPoll_ProvidesLinkForAddingOptions()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));

                Assert.IsTrue(addOptionLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void OptionAddingLink_PromptsForOptionDetails()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));
                addOptionLink.Click();

                Assert.AreEqual(PollUrl, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);

                IWebElement formDescription = _driver.FindElement(NgBy.Model("addOptionForm.description"));
                Assert.IsTrue(formDescription.IsVisible());
                Assert.AreEqual(String.Empty, formDescription.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void OptionAddingPrompt_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));
                addOptionLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
                IWebElement addButton = _driver.FindElement(By.Id("add-button"));

                Assert.IsTrue(addButton.IsVisible());
                Assert.IsFalse(addButton.Enabled);

                formName.SendKeys("New Option");

                Assert.IsTrue(addButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void OptionAddingSubmission_AddsOption()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));
                addOptionLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
                IWebElement addButton = _driver.FindElement(By.Id("add-button"));

                String newOptionName = "New Option";
                formName.SendKeys(newOptionName);

                IWebElement formButton = _driver.FindElement(By.Id("add-button"));
                formButton.Click();

                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingMultiPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);

                // Refresh to ensure the new option was stored in DB
                _driver.Navigate().GoToUrl(PollUrl);

                optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingMultiPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);

                _optionAddingMultiPoll.Options.Remove(_optionAddingMultiPoll.Options.Last());
                _context.SaveChanges();
            }
        }

        [TestClass]
        public class HiddenResultsConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _hiddenResultsMultiPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Option> testPollOptions = new List<Option>() {
                    new Option()
                    { 
                        Name = "Test Option 1", Description = "Test Description 1" 
                    }
                };

                // Open, Anonymous, No Option Adding, Shown Results
                _hiddenResultsMultiPoll = new Poll()
                {
                    UUID = PollGuid,
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
            public void HiddenResultsPoll_DoesNotShowResultsButton()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement resultButton = _driver.FindElement(By.Id("results-button"));

                Assert.IsFalse(resultButton.IsVisible());
            }
        }
    }
}
