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
    [TestClass]
    public class ManageBallotsTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
        private static readonly int WaitTime = 500;
        private static readonly Guid PollGuid = Guid.NewGuid();
        private static readonly Guid PollManageGuid = Guid.NewGuid();
        private static readonly string PollUrl = SiteBaseUri + "Dashboard/#/Manage/" + PollManageGuid + "/Voters";

        private ITestVotingContext _context;
        private Poll _defaultPoll;
        private IWebDriver _driver;

        [TestInitialize]
        public virtual void TestInitialise()
        {
            _context = new TestVotingContext();

            // Open, Anonymous, No Choice Adding, Shown Results
            _defaultPoll = new Poll()
            {
                UUID = PollGuid,
                ManageId = PollManageGuid,
                PollType = PollType.Basic,
                Name = "Test Poll",
                LastUpdatedUtc = DateTime.UtcNow,
                CreatedDateUtc = DateTime.UtcNow,
                Choices = new List<Choice>(),
                InviteOnly = false,
                NamedVoting = false,
                ChoiceAdding = false,
                IsElectionMode = false,
                MaxPerVote = 3,
                MaxPoints = 4,
                Ballots = new List<Ballot>()
            };

            Choice testChoice = new Choice() { Name = "Test", PollChoiceNumber = 1 };

            _defaultPoll.Choices.Add(testChoice);

            Ballot ballot1 = new Ballot()
            {
                TokenGuid = Guid.NewGuid(),
                ManageGuid = Guid.NewGuid(),
                VoterName = "Voter 1",
                Votes = new List<Vote>()
            };

            Ballot ballot2 = new Ballot()
            {
                TokenGuid = Guid.NewGuid(),
                ManageGuid = Guid.NewGuid(),
                VoterName = "Voter 2",
                Votes = new List<Vote>()
            };

            ballot1.Votes.Add(new Vote()
            {
                Choice = testChoice,
                Ballot = ballot1,
                VoteValue = 1
            });

            ballot2.Votes.Add(new Vote()
            {
                Choice = testChoice,
                Ballot = ballot2,
                VoteValue = 1
            });

            _defaultPoll.Ballots.Add(ballot1);
            _defaultPoll.Ballots.Add(ballot2);

            _context.Polls.Add(_defaultPoll);
            _context.SaveChanges();

            _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
            _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
            _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _driver.Dispose();

            using (ITestVotingContext clearContext = new TestVotingContext())
            {
                PollClearer pollTearDown = new PollClearer(clearContext);
                pollTearDown.ClearPoll(_defaultPoll);
            }

            _context.Dispose();
        }

        [TestMethod, TestCategory("E2E")]
        public void CancelButton_NavigatesToManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));
            cancelButton.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId, _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void CancelButton_DoesNotSaveChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement clearAllButton = _driver.FindElement(By.Id("clear-all-button"));
            clearAllButton.Click();

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));
            cancelButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == PollManageGuid).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(_defaultPoll.Ballots.Count, dbPoll.Ballots.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ClearAllButton_ClearsAllBallots()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement clearAllButton = _driver.FindElement(By.Id("clear-all-button"));
            clearAllButton.Click();

            IWebElement saveButton = _driver.FindElement(By.Id("save-button"));
            saveButton.Click();

            // I have no idea why we need a new context, but verification of row deletions fails if we use the existing context
            using (ITestVotingContext testContext = new TestVotingContext())
            {
                Poll dbPoll = testContext.Polls.Where(p => p.ManageId == PollManageGuid).Single();
                Assert.AreEqual(0, dbPoll.Ballots.Count);
            }

        }

        [TestMethod, TestCategory("E2E")]
        public void ClearBallotButton_ClearsBallot()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> clearAllButtons = _driver.FindElements(By.Id("remove-ballot-button"));
            clearAllButtons.First().Click();

            IWebElement saveButton = _driver.FindElement(By.Id("save-button"));
            saveButton.Click();

            // I have no idea why we need a new context, but verification of row deletions fails if we use the existing context
            using (ITestVotingContext testContext = new TestVotingContext())
            {
                Poll dbPoll = testContext.Polls.Where(p => p.ManageId == PollManageGuid).Single();
                Assert.AreEqual(0, dbPoll.Ballots.Count);
            }

        }
    }
}
