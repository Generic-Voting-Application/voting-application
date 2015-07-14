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
    public class BasicPollTests
    {
        private const string ChromeDriverDir = @"..\..\";
        private const string SiteBaseUri = @"http://localhost:64205/";

        [TestClass]
        public class DefaultPollConfiguration : E2ETest
        {
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollVoteUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";
            private static readonly string PollResultsUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Results";

            [TestMethod]
            [TestCategory("E2E")]
            public void PopulatedChoices_DisplaysAllChoices()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreatePoll(context);
                        GoToUrl(driver, PollVoteUrl);

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
            public void VotingOnChoice_NavigatesToResultsPage()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollVoteUrl);

                        IReadOnlyCollection<IWebElement> voteButtons = FindElementsById(driver, "vote-button");
                        voteButtons.First().Click();

                        Assert.IsTrue(driver.Url.StartsWith(PollResultsUrl));
                    }
                }
            }

            [Ignore]
            [TestMethod]
            [TestCategory("E2E")]
            public void DefaultPoll_ShowsResultsButton()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollVoteUrl);

                        IWebElement resultsLink = FindElementById(driver, "results-button");

                        Assert.IsTrue(resultsLink.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void AfterVoting_VoteIsRemembered()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollVoteUrl);

                        IWebElement voteButtons = FindElementById(driver, "vote-button");
                        voteButtons.Click();

                        NavigateBackToVotePage(driver);

                        IWebElement selectedChoice = driver.FindElement(By.CssSelector(".selected-choice"));

                        Assert.IsTrue(selectedChoice.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void ClearVote_RemovesVote()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreatePoll(context);
                        GoToUrl(driver, PollVoteUrl);

                        IWebElement voteButtons = FindElementById(driver, "vote-button");
                        voteButtons.Click();

                        NavigateBackToVotePage(driver);

                        IWebElement clearVoteChoice = FindElementById(driver, "clear-vote-button");
                        clearVoteChoice.Click();

                        NavigateBackToVotePage(driver);


                        By selectedChoiceLocator = By.CssSelector(".selected-choice");

                        Assert.IsFalse(ElementExists(driver, selectedChoiceLocator));
                    }
                }
            }

            public static Poll CreatePoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() 
                {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                var defaultBasicPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Basic,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = false
                };

                testContext.Polls.Add(defaultBasicPoll);
                testContext.SaveChanges();

                return defaultBasicPoll;
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
            private static readonly string PollUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";

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

                        string expectedUrlPrefix = SiteBaseUri + "Poll/#/" + poll.UUID + "/Results";
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
                var inviteOnlyBasicPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Basic,
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

                testContext.Polls.Add(inviteOnlyBasicPoll);
                testContext.SaveChanges();

                return inviteOnlyBasicPoll;
            }
        }

        [TestClass]
        public class NamedVotingTests : E2ETest
        {
            const string VoterName = "User";
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollVoteUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";
            private static readonly string PollResultsUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Results";

            [TestMethod]
            [TestCategory("E2E")]
            public void NoNameEntered_VoteNotAllowed()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, PollVoteUrl);


                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        Assert.IsFalse(driver.Url.StartsWith(PollResultsUrl));
                        Assert.IsTrue(driver.Url.StartsWith(PollVoteUrl));
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void NameEntered_VoteAllowed()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, PollVoteUrl);


                        IWebElement nameInput = FindElementById(driver, "voter-name-input");
                        nameInput.SendKeys(VoterName);

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        Assert.IsTrue(driver.Url.StartsWith(PollResultsUrl));
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void NoNameEntered_ShowsFailedValidationMessage()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, PollVoteUrl);


                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        IWebElement requiredValidationMessage = FindElementById(driver, "voter-name-required-validation-message");

                        Assert.IsTrue(requiredValidationMessage.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void EnteringVoterName_AllowsVoting()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, PollVoteUrl);


                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        IWebElement requiredValidationMessage = FindElementById(driver, "voter-name-required-validation-message");

                        Assert.IsTrue(requiredValidationMessage.IsVisible());



                        IWebElement nameInput = FindElementById(driver, "voter-name-input");
                        nameInput.SendKeys(VoterName);

                        voteButton.Click();

                        Assert.IsTrue(driver.Url.StartsWith(PollResultsUrl));
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void VotingAndReturning_RemembersVoterName()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateNamedVotersPoll(context);
                        GoToUrl(driver, PollVoteUrl);


                        IWebElement nameInput = FindElementById(driver, "voter-name-input");
                        nameInput.SendKeys(VoterName);

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        GoToUrl(driver, PollVoteUrl);

                        IWebElement newNameInput = FindElementById(driver, "voter-name-input");

                        Assert.AreEqual(VoterName, newNameInput.GetAttribute("value"));
                    }
                }
            }

            public static Poll CreateNamedVotersPoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() 
                {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                var namedVotersPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Basic,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = true,
                    ChoiceAdding = false,
                    ElectionMode = false
                };

                testContext.Polls.Add(namedVotersPoll);
                testContext.SaveChanges();

                return namedVotersPoll;
            }
        }

        [TestClass]
        public class ChoiceAddingPollConfiguration
        {
            private static ITestVotingContext _context;
            private static Poll _choiceAddingBasicPoll;
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
                _choiceAddingBasicPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Basic,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = true,
                    ElectionMode = false
                };

                _context.Polls.Add(_choiceAddingBasicPoll);
                _context.SaveChanges();
            }

            [ClassCleanup]
            public static void ClassCleanup()
            {
                PollClearer pollTearDown = new PollClearer(_context);
                pollTearDown.ClearPoll(_choiceAddingBasicPoll);

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
                const string newChoiceName = "New Choice";
                formName.SendKeys(newChoiceName);

                IWebElement formButton = _driver.FindElement(By.Id("add-button"));
                formButton.Click();

                IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_choiceAddingBasicPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                // Refresh to ensure the new choice was stored in DB
                _driver.Navigate().GoToUrl(PollUrl);

                choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

                Assert.AreEqual(_choiceAddingBasicPoll.Choices.Count + 1, choiceNames.Count);
                Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                _choiceAddingBasicPoll.Choices.Remove(_choiceAddingBasicPoll.Choices.Last());
                _context.SaveChanges();
            }
        }

        [TestClass]
        public class ElectionModeConfiguration : E2ETest
        {
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollVoteUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";
            private static readonly string PollResultsUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";

            [Ignore]
            [TestMethod]
            [TestCategory("E2E")]
            public void ElectionModePoll_DoesNotShowResultsButton()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreateElectionPoll(context);
                        GoToUrl(driver, PollVoteUrl);

                        IWebElement resultButton = FindElementById(driver, "results-button");

                        Assert.IsFalse(resultButton.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void ElectionModePoll_WithVotes_NavigatesToResults()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateElectionPoll(context);
                        GoToUrl(driver, PollVoteUrl);

                        IReadOnlyCollection<IWebElement> voteButtons = FindElementsById(driver, "vote-button");
                        voteButtons.First().Click();

                        GoToUrl(driver, PollVoteUrl);

                        WaitForPageChange();

                        Assert.IsTrue(driver.Url.StartsWith(PollResultsUrl));
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void ElectionModeResults_WithoutVotes_NavigatesToVote()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreateElectionPoll(context);

                        string resultsUrl = PollVoteUrl.Replace("Vote", "Results");

                        GoToUrl(driver, PollVoteUrl);

                        WaitForPageChange();

                        Assert.AreEqual(PollVoteUrl, driver.Url);
                    }
                }
            }

            public static Poll CreateElectionPoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() 
                {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                };

                // Open, Anonymous, No Choice Adding, Shown Results
                var electionBasicPoll = new Poll()
                {
                    UUID = PollGuid,
                    PollType = PollType.Basic,
                    Name = "Test Poll",
                    LastUpdatedUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow,
                    Choices = testPollChoices,
                    InviteOnly = false,
                    NamedVoting = false,
                    ChoiceAdding = false,
                    ElectionMode = true
                };

                testContext.Polls.Add(electionBasicPoll);
                testContext.SaveChanges();

                return electionBasicPoll;
            }
        }
    }
}
