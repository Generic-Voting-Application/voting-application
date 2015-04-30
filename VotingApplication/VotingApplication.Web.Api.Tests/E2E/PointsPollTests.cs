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
        private static readonly int WaitTime = 500;

        #region Default Config
        [TestClass]
        public class DefaultPollConfiguration
        {
            private static IVotingContext _context;

            private const int truncatedTextLimit = 60;
            private static Poll _defaultPointsPoll;

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
                _defaultPointsPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
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
            public void PointsPoll_PopulatedOptions_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_defaultPointsPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_defaultPointsPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_PopulatedOptions_DisplaysAllOptionDescriptions()
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
            public void PointsPoll_VotingOnOption_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                firstOptionButtons.First(b => b.Text == "+").Click();
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _defaultPointsPoll.UUID));
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_DefaultPoll_ShowsResultsButton()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement resultsLink = anchors.FirstOrDefault(a => a.Text == "Go to results");

                Assert.IsTrue(resultsLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_PointsButtons_ChangePointAllocations()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                IWebElement selectedOptionPoints = options.First().FindElement(NgBy.Binding("option.voteValue"));

                IWebElement plusButton = firstOptionButtons.First(b => b.Text == "+");
                IWebElement minusButton = firstOptionButtons.First(b => b.Text == "-");

                Assert.IsTrue(selectedOptionPoints.IsVisible());
                Assert.AreEqual("0/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);

                plusButton.Click();

                Assert.AreEqual("1/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);

                minusButton.Click();

                Assert.AreEqual("0/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_PointsButtons_ChangeTotalPointLimit()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                IWebElement totalPoints = _driver.FindElement(NgBy.Binding("poll.MaxPoints"));

                IWebElement plusButton = firstOptionButtons.First(b => b.Text == "+");
                IWebElement minusButton = firstOptionButtons.First(b => b.Text == "-");

                Assert.IsTrue(totalPoints.IsVisible());
                Assert.AreEqual("Unallocated points: " + _defaultPointsPoll.MaxPoints + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);

                plusButton.Click();

                Assert.AreEqual("Unallocated points: " + (_defaultPointsPoll.MaxPoints - 1) + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);

                minusButton.Click();

                Assert.AreEqual("Unallocated points: " + _defaultPointsPoll.MaxPoints + "/" + _defaultPointsPoll.MaxPoints, totalPoints.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_PointsPerOption_CanNotExceedMaximum()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));
                IWebElement plusButton = firstOptionButtons.First(b => b.Text == "+");

                for (int i = 0; i < _defaultPointsPoll.MaxPerVote; i++)
                {
                    plusButton.Click();
                }

                Assert.IsFalse(plusButton.Enabled);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_TotalPoints_CanNotExceedMaximum()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                int allocatedPoints = 0;

                foreach (IWebElement option in options)
                {
                    IReadOnlyCollection<IWebElement> optionButtons = option.FindElements(By.TagName("Button"));
                    IWebElement plusButton = optionButtons.First(b => b.Text == "+");

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
                    IReadOnlyCollection<IWebElement> optionButtons = option.FindElements(By.TagName("Button"));
                    IWebElement plusButton = optionButtons.First(b => b.Text == "+");

                    Assert.IsFalse(plusButton.Enabled);
                }
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsVote_AfterVoting_VoteIsRemembered()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _defaultPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));

                IReadOnlyCollection<IWebElement> firstOptionButtons = options.First().FindElements(By.TagName("Button"));

                firstOptionButtons.First(b => b.Text == "+").Click();
                buttons.First(b => b.Text == "Vote").Click();

                _driver.Navigate().GoToUrl(_driver.Url.Replace("Results", "Vote"));

                options = _driver.FindElements(NgBy.Repeater("option in poll.Options"));
                IWebElement selectedOptionPoints = options.First().FindElement(NgBy.Binding("poll.MaxPerVote"));

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(selectedOptionPoints.IsVisible());
                Assert.AreEqual("1/" + _defaultPointsPoll.MaxPerVote, selectedOptionPoints.Text);
            }
        }
        #endregion

        #region Invite Only Config
        [TestClass]
        public class InviteOnlyPollConfiguration
        {
            private static IVotingContext _context;

            private static Poll _inviteOnlyPointsPoll;

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
                _inviteOnlyPointsPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
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

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_AccessWithNoToken_DisplaysError()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyPointsPoll.UUID);
                IWebElement error = _driver.FindElement(NgBy.Binding("$root.error.readableMessage"));

                Assert.IsTrue(error.IsVisible());
                Assert.AreEqual("This poll is invite only", error.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_AccessWithToken_DisplaysAllOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyPointsPoll.UUID + "/" + _inviteOnlyPointsPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_inviteOnlyPointsPoll.Options.Count, optionNames.Count);
                CollectionAssert.AreEquivalent(_inviteOnlyPointsPoll.Options.Select(o => o.Name).ToList(),
                                               optionNames.Select(o => o.Text).ToList());
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_VoteWithToken_NavigatesToResultsPage()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _inviteOnlyPointsPoll.UUID + "/" + _inviteOnlyPointsPoll.Ballots[0].TokenGuid);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                VoteClearer voterClearer = new VoteClearer(_context);
                voterClearer.ClearLast();

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _inviteOnlyPointsPoll.UUID));
            }
        }
        #endregion

        #region Named Voters Config
        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static IVotingContext _context;

            private static Poll _namedPointsPoll;

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
                _namedPointsPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
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

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_VoteWithNoName_PromptsForName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
                buttons.First(b => b.Text == "Vote").Click();

                Assert.AreEqual(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID, _driver.Url);

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
                Assert.IsTrue(formName.IsVisible());
                Assert.AreEqual(String.Empty, formName.Text);
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_NameInput_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID);
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
            public void PointsPoll_NameInput_VotesUponSubmission()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _namedPointsPoll.UUID);
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

                Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _namedPointsPoll.UUID));
            }
        }
        #endregion

        #region Option Adding Config
        [TestClass]
        public class OptionAddingPollConfiguration
        {
            private static IVotingContext _context;

            private static Poll _optionAddingPointsPoll;

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
                _optionAddingPointsPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
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

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_OptionAddingPoll_ProvidesLinkForAddingOptions()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");

                Assert.IsTrue(addOptionLink.IsVisible());
            }

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_OptionAddingLink_PromptsForOptionDetails()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement addOptionLink = anchors.First(a => a.Text == "+ Add another option");
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
            public void PointsPoll_OptionAddingPrompt_AcceptsValidName()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
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
            public void PointsPoll_OptionAddingSubmission_AddsOption()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);
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

                Assert.AreEqual(_optionAddingPointsPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);

                // Refresh to ensure they new option was stored in DB
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _optionAddingPointsPoll.UUID);

                optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

                Assert.AreEqual(_optionAddingPointsPoll.Options.Count + 1, optionNames.Count);
                Assert.AreEqual(newOptionName, optionNames.Last().Text);
            }
        }
        #endregion

        #region Hidden Results Config
        [TestClass]
        public class HiddenResultsConfiguration
        {
            private static IVotingContext _context;

            private static Poll _hiddenResultsPointsPoll;

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
                _hiddenResultsPointsPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
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

            [TestMethod, TestCategory("E2E")]
            public void PointsPoll_HiddenResultsPoll_DoesNotShowResultsButton()
            {
                _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _hiddenResultsPointsPoll.UUID);
                IReadOnlyCollection<IWebElement> anchors = _driver.FindElements(By.TagName("a"));
                IWebElement resultsLink = anchors.FirstOrDefault(a => a.Text == "Go to results");

                Assert.IsNull(resultsLink);
            }
        }
        #endregion
    }
}
