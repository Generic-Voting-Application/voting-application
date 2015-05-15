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

                List<Option> testPollOptions = new List<Option>() {
                    new Option(){ Name = "Test Option 1", Description = "Test Description 1" }
                };

                // Open, Anonymous, No Option Adding, Shown Results
                _defaultPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = false,
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
            public void PopulatedOptions_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_defaultPointsPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_defaultPointsPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PopulatedOptions_DisplaysAllOptionDescriptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Model("option.Description"));

                Assert.AreEqual(_defaultPointsPoll.Options.Count, optionDescriptions.Count);
                List<String> expectedDescriptions = new List<string>();

                foreach (Option option in _defaultPointsPoll.Options)
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
            public void VotingOnOption_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> increaseOptionButtons = _driver.FindElements(By.Id("increase-button"));
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));

                increaseOptionButtons.First().Click();
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
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                IWebElement selectedOptionPoints = options.First().FindElement(NgBy.Binding("option.voteValue"));
                IReadOnlyCollection<IWebElement> increaseOptionButtons = _driver.FindElements(By.Id("increase-button"));
                IReadOnlyCollection<IWebElement> decreaseOptionButtons = _driver.FindElements(By.Id("decrease-button"));

                IWebElement plusButton = increaseOptionButtons.First();
                IWebElement minusButton = decreaseOptionButtons.First();

                Assert.IsTrue(selectedOptionPoints.IsVisible());
                Assert.AreEqual("0/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);

                plusButton.Click();

                Assert.AreEqual("1/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);

                minusButton.Click();

                Assert.AreEqual("0/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsButtons_ChangeTotalPointLimit()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                IWebElement totalPoints = _driver.FindElement(NgBy.Binding("poll.MaxPoints"));

                IReadOnlyCollection<IWebElement> increaseOptionButtons = _driver.FindElements(By.Id("increase-button"));
                IReadOnlyCollection<IWebElement> decreaseOptionButtons = _driver.FindElements(By.Id("decrease-button"));

                IWebElement plusButton = increaseOptionButtons.First();
                IWebElement minusButton = decreaseOptionButtons.First();

                Assert.IsTrue(totalPoints.IsVisible());
                Assert.AreEqual("Unallocated points: " + _defaultPointsPoll.MaxPoints + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);

                plusButton.Click();

                Assert.AreEqual("Unallocated points: " + (_defaultPointsPoll.MaxPoints - 1) + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);

                minusButton.Click();

                Assert.AreEqual("Unallocated points: " + _defaultPointsPoll.MaxPoints + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPerOption_CanNotExceedMaximum()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> increaseOptionButtons = _driver.FindElements(By.Id("increase-button"));

                IWebElement plusButton = increaseOptionButtons.First();

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
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                int allocatedPoints = 0;

                foreach (IWebElement option in options)
                {
                    IReadOnlyCollection<IWebElement> increaseOptionButtons = _driver.FindElements(By.Id("increase-button"));

                    IWebElement plusButton = increaseOptionButtons.First();

                    for (int i = 0; i < _defaultPointsPoll.MaxPerVote; i++)
                    {
                        if (allocatedPoints < _defaultPointsPoll.MaxPoints)
                        {
                            plusButton.Click();
                            allocatedPoints++;
                        }
                    }
                }

                foreach (IWebElement option in options)
                {
                    IWebElement increaseButton = option.FindElement(By.Id("increase-button"));

                    Assert.IsFalse(increaseButton.Enabled);
                }
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsVote_AfterVoting_VoteIsRemembered()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> increaseOptionButtons = _driver.FindElements(By.Id("increase-button"));

                IWebElement plusButton = increaseOptionButtons.First();
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));

                plusButton.Click();
                voteButton.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));
                IWebElement selectedOptionPoints = options.First().FindElement(NgBy.Binding("poll.MaxPerVote"));

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(selectedOptionPoints.IsVisible());
                Assert.AreEqual("1/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void ClearVote_RemovesVote()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> increaseOptionButtons = _driver.FindElements(By.Id("increase-button"));

                IWebElement plusButton = increaseOptionButtons.First();
                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));

                plusButton.Click();
                voteButton.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                IWebElement clearVoteOption = _driver.FindElements(By.Id("clear-vote-button")).Single();
                clearVoteOption.Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));
                IWebElement selectedOptionPoints = options.First().FindElement(NgBy.Binding("poll.MaxPerVote"));

                Assert.IsTrue(selectedOptionPoints.IsVisible());
                Assert.AreEqual("0/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);
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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Invite Only, Anonymous, No Option Adding, Shown Results
                _inviteOnlyPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
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
            public void AccessWithToken_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyPointsPoll.UUID + "/" + _inviteOnlyPointsPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_inviteOnlyPointsPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyPointsPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
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

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", Description = "Test Description 2" }};

                // Open, Named voters, No Option Adding, Shown Results
                _namedPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = true,
                    OptionAdding = false,
                    HiddenResults = false,
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
        public class OptionAddingPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _optionAddingPointsPoll;
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
                _optionAddingPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = true,
                    HiddenResults = false,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_optionAddingPointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_optionAddingPointsPoll);

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
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));

                Assert.IsTrue(addOptionLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void OptionAddingLink_PromptsForOptionDetails()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));
                addOptionLink.Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID, _driver.Url);

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
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));
                addOptionLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement doneButton = _driver.FindElement(By.Id("done-button"));

                Assert.IsTrue(doneButton.IsVisible());
                Assert.IsFalse(doneButton.Enabled);

                formName.SendKeys("New Option");

                Assert.IsTrue(doneButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void OptionAddingSubmission_AddsOption()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
                IWebElement addOptionLink = _driver.FindElement(By.Id("add-option-link"));
                addOptionLink.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IWebElement doneButton = _driver.FindElement(By.Id("done-button"));

                String newOptionName = "New Option";
                formName.SendKeys(newOptionName);

                IWebElement form = _driver.FindElement(By.Name("addOptionForm"));
                form.Submit();

                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingPointsPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);

                // Refresh to ensure they new option was stored in DB
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);

                optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingPointsPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);
            }
        }

        [TestClass]
        public class HiddenResultsConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _hiddenResultsPointsPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;
            private IWebDriver _driver;

            [ClassInitialize]
            public static void ClassInitialise(TestContext testContext)
            {
                _context = new TestVotingContext();

                List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", 
                              Description = "A very long test description 2 that should exceed the character limit for descriptions" }};

                // Open, Anonymous, No Option Adding, Shown Results
                _hiddenResultsPointsPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Points,
                    Name = "Test Poll",
                    LastUpdated = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Options = testPollOptions,
                    InviteOnly = false,
                    NamedVoting = false,
                    OptionAdding = false,
                    HiddenResults = true,
                    MaxPerVote = 3,
                    MaxPoints = 4
                };

                _context.Polls.Add(_hiddenResultsPointsPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_hiddenResultsPointsPoll);

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
