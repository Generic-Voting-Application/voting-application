using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using VotingApplication.Data.Model;
using VotingApplication.Web.Tests.E2E.Helpers;

namespace VotingApplication.Web.Tests.E2E
{
    public class BasicPollTests
    {
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

                        IReadOnlyCollection<IWebElement> choices = driver.FindElements(NgBy.Binding("choice.Name"));
                        choices.First().Click();

                        IWebElement voteButton = FindElementById(driver, "vote-button");
                        voteButton.Click();

                        NavigateBackToVotePage(driver);

                        IWebElement selectedChoice = driver.FindElement(By.CssSelector(".md-ripple-visible"));

                        Assert.IsTrue(selectedChoice.IsVisible());
                    }
                }
            }

            [Ignore]
            [TestMethod]
            [TestCategory("E2ERewrite")]
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
                    new Choice(){ Name = "TEST CHOICE 1", Description = "Test Description 1" },
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
                    new Choice() { Name = "TEST CHOICE 1", Description = "Test Description 1"},
                    new Choice() { Name = "TEST CHOICE 2", Description = "Test Description 2"}
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
        public class ChoiceAddingTests : E2ETest
        {
            private static readonly Guid PollGuid = Guid.NewGuid();
            private static readonly string PollVoteUrl = SiteBaseUri + "Poll/#/" + PollGuid + "/Vote";

            [TestMethod]
            [TestCategory("E2E")]
            public void ChoiceAddingPoll_UserChoiceInputVisible()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        CreateChoiceAddingPoll(context);

                        GoToUrl(driver, PollVoteUrl);

                        IWebElement addChoiceInput = FindElementById(driver, "user-choice-input");

                        Assert.IsTrue(addChoiceInput.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void NonChoiceAddingPoll_UserChoiceInputNotVisible()
            {
                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreateChoiceAddingPoll(context);
                        poll.ChoiceAdding = false;
                        context.SaveChanges();

                        GoToUrl(driver, PollVoteUrl);

                        IWebElement addChoiceInput = FindElementById(driver, "user-choice-input");

                        Assert.IsFalse(addChoiceInput.IsVisible());
                    }
                }
            }

            [TestMethod]
            [TestCategory("E2E")]
            public void Add_AddsChoice()
            {
                const string newChoiceName = "NEW CHOICE";

                using (IWebDriver driver = Driver)
                {
                    using (var context = new TestVotingContext())
                    {
                        Poll poll = CreateChoiceAddingPoll(context);

                        GoToUrl(driver, PollVoteUrl);

                        IWebElement addChoiceInput = FindElementById(driver, "user-choice-input");
                        addChoiceInput.SendKeys(newChoiceName);

                        IWebElement formButton = FindElementById(driver, "add-user-choice-button");
                        formButton.Click();

                        IReadOnlyCollection<IWebElement> choiceNames = driver.FindElements(NgBy.Binding("choice.Name"));

                        Assert.AreEqual(poll.Choices.Count + 1, choiceNames.Count);
                        Assert.AreEqual(newChoiceName, choiceNames.Last().Text);

                        // Refresh to ensure the new choice was stored in DB
                        GoToUrl(driver, PollVoteUrl);

                        IReadOnlyCollection<IWebElement> choiceNamesAfterRefresh = driver.FindElements(NgBy.Binding("choice.Name"));

                        Assert.AreEqual(poll.Choices.Count + 1, choiceNamesAfterRefresh.Count);
                        Assert.AreEqual(newChoiceName, choiceNamesAfterRefresh.Last().Text);
                    }
                }
            }

            public static Poll CreateChoiceAddingPoll(TestVotingContext testContext)
            {
                var testPollChoices = new List<Choice>() 
                {
                    new Choice(){ Name = "Test Choice 1", Description = "Test Description 1" },
                };

                // Open, Anonymous, Choice Adding, Shown Results
                var choiceAddingBasicPoll = new Poll()
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

                testContext.Polls.Add(choiceAddingBasicPoll);
                testContext.SaveChanges();

                return choiceAddingBasicPoll;
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
