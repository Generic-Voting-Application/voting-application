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
    public class UpDownPollTests
    {
        private const string ChromeDriverDir = @"..\..\";
        private const string SiteBaseUri = @"http://localhost:64205/";

        [TestClass]
        public class DefaultPollConfiguration : E2ETest
        {
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;

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
            public void PopulatedChoices_DisplaysChoiceDescriptions()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> choiceDescriptions = driver.FindElements(NgBy.Model("choice.Description"));

                        Assert.AreEqual(poll.Choices.Count, choiceDescriptions.Count);

                        List<string> expected = poll.Choices.Select(o => o.Description).ToList();
                        List<string> actual = choiceDescriptions.Select(o => o.Text).ToList();
                        CollectionAssert.AreEquivalent(expected, actual);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void PopulatedChoices_TruncatesLongDescriptionsContainingAtLeastOneSpace()
            {
                const int truncationLength = 30;
                string expectedText = new String('a', truncationLength) + "... Show More";

                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);

                        string tooLongChoiceDescription = new String('a', truncationLength) + " " + new String('a', truncationLength);

                        poll.Choices.Add(new Choice()
                        {
                            Name = "Test Choice 2",
                            Description = tooLongChoiceDescription + " " + tooLongChoiceDescription
                        });
                        context.SaveChanges();


                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> choiceDescriptions = driver.FindElements(NgBy.Model("choice.Description"));

                        string selectedChoiceText = choiceDescriptions.Select(o => o.Text).Last();
                        Assert.AreEqual(expectedText, selectedChoiceText);
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void PopulatedChoices_DoesNotTruncateLongDescriptionsContainingNoSpaces()
            {
                const int longDescriptionLength = 50;
                string expectedText = new String('a', longDescriptionLength);

                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);

                        string tooLongChoiceDescription = new String('a', longDescriptionLength);

                        poll.Choices.Add(new Choice()
                        {
                            Name = "Test Choice 2",
                            Description = tooLongChoiceDescription
                        });
                        context.SaveChanges();


                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> choiceDescriptions = driver.FindElements(NgBy.Model("choice.Description"));

                        string selectedChoiceText = choiceDescriptions.Select(o => o.Text).Last();
                        Assert.AreEqual(expectedText, selectedChoiceText);
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
                        IReadOnlyCollection<IWebElement> choices = driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                        IReadOnlyCollection<IWebElement> firstChoiceButtons = choices.First().FindElements(By.TagName("Button"));
                        IWebElement downButton = firstChoiceButtons.First();
                        downButton.Click();

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        string expectedUriPrefix = SiteBaseUri + "Poll/#/Results/" + poll.UUID;
                        Assert.IsTrue(driver.Url.StartsWith(expectedUriPrefix));
                    }
                }
            }

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

            [TestMethod, TestCategory("E2E")]
            public void AfterVoting_VoteIsRemembered()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollUrl);

                        IReadOnlyCollection<IWebElement> choices = driver.FindElements(NgBy.Repeater("choice in poll.Choices"));

                        IReadOnlyCollection<IWebElement> firstChoiceButtons = choices.First().FindElements(By.TagName("Button"));
                        IWebElement downButton = firstChoiceButtons.First();
                        downButton.Click();

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        NavigateBackToVotePage(driver);

                        IReadOnlyCollection<IWebElement> selectedChoices = driver.FindElements(NgBy.Repeater("choice in poll.Choices"));
                        IWebElement selectedChoice = selectedChoices.First();
                        IWebElement selectedChoiceButton = selectedChoice.FindElement(By.CssSelector(".active-btn"));


                        Assert.IsTrue(selectedChoiceButton.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void NavigatingToNonExistentPoll_ShowsErrorPage()
            {
                using (IWebDriver driver = Driver)
                {
                    GoToUrl(driver, PollUrl);

                    IWebElement errorDirective = FindElementById(driver, "voting-partial-error");

                    Assert.IsTrue(errorDirective.IsVisible());
                }
            }

            public static Poll CreatePoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() 
                {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" }
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                var defaultUpDownPoll = new Poll()
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
                    ElectionMode = false
                };

                testContext.Polls.Add(defaultUpDownPoll);
                testContext.SaveChanges();

                return defaultUpDownPoll;
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
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/Vote/" + PollGuid;

            [TestMethod]
            [TestCategory("E2E")]
            public void AccessWithNoToken_DisplaysErrorPage()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);

                        GoToUrl(driver, PollUrl);

                        IWebElement errorDirective = FindElementById(driver, "voting-partial-error");

                        Assert.IsTrue(errorDirective.IsVisible());
                    }
                }
            }

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

                        string expectedUrlPrefix = SiteBaseUri + "Poll/#/Results/" + poll.UUID;
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
                var inviteOnlyUpDownPoll = new Poll()
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
                    ElectionMode = false,
                    Ballots = new List<Ballot>()
                    {
                        new Ballot() { TokenGuid = Guid.NewGuid() }
                    }
                };

                testContext.Polls.Add(inviteOnlyUpDownPoll);
                testContext.SaveChanges();

                return inviteOnlyUpDownPoll;
            }
        }

        [TestClass]
        public class NamedVotersPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _namedUpDownPoll;
            private static readonly Guid PollGuid = Guid.NewGuid();
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
                    ElectionMode = false
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

                IWebElement voteButton = _driver.FindElement(By.Id("vote-button"));
                voteButton.Click();

                IWebElement formName = _driver.FindElement(NgBy.Model("loginForm.name"));
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
                    ElectionMode = false
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


                const string newChoiceName = "New Choice";
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
        public class ElectionModeConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _electionModeUpDownPoll;
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
                _electionModeUpDownPoll = new Poll()
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
                    ElectionMode = true
                };

                _context.Polls.Add(_electionModeUpDownPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_electionModeUpDownPoll);

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
            public void ElectionModePoll_DoesNotShowResultsButton()
            {
                _driver.Navigate().GoToUrl(PollUrl);
                IWebElement resultButton = _driver.FindElement(By.Id("results-button"));

                Assert.IsFalse(resultButton.IsVisible());
            }
        }
    }
}
