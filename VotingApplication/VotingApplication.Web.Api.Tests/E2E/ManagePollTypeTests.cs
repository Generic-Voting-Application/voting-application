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
    public class ManagePollTypeTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
        private static readonly int WaitTime = 500;
        private static readonly Guid PollGuid = Guid.NewGuid();
        private static readonly Guid PollManageGuid = Guid.NewGuid();
        private static readonly string PollUrl = SiteBaseUri + "Dashboard/#/Manage/" + PollManageGuid + "/PollType";

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
                MaxPoints = 4
            };

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

            PollClearer pollTearDown = new PollClearer(_context);
            pollTearDown.ClearPoll(_defaultPoll);
            _context.Dispose();
        }

        [TestMethod, TestCategory("E2E")]
        public void ManagePollType_CancelButton_NavigatesToManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId, _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManagePollType_CancelButton_DoesNotSaveChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement multiVoteButton = _driver.FindElement(By.Id("multi-vote-button"));
            multiVoteButton.Click();

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.UUID == _defaultPoll.UUID).Single();

            Assert.AreEqual(PollType.Basic, dbPoll.PollType);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManagePollType_Save_SavesChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement multiVoteButton = _driver.FindElement(By.Id("multi-vote-button"));

            multiVoteButton.Click();

            IWebElement saveButton = _driver.FindElement(By.Id("save-button"));

            saveButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(PollType.Multi, dbPoll.PollType);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManagePollType_Save_SavesPollConfigChanges()
        {
            int? initialMaxPoints = _defaultPoll.MaxPoints;
            int? initialMaxPerVote = _defaultPoll.MaxPerVote;

            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement pointsVoteButton = _driver.FindElement(By.Id("points-vote-button"));

            IWebElement pointsPerPollControl = _driver.FindElement(By.Id("points-per-poll"));
            IWebElement pointsPerPollIncreaseButton = pointsPerPollControl.FindElement(By.Id("increase-button"));

            IWebElement pointsPerVoteControl = _driver.FindElement(By.Id("points-per-vote"));
            IWebElement pointsPerVoteDecreaseButton = pointsPerVoteControl.FindElement(By.Id("decrease-button"));

            pointsVoteButton.Click();
            pointsPerPollIncreaseButton.Click();
            pointsPerVoteDecreaseButton.Click();

            IWebElement saveButton = _driver.FindElement(By.Id("save-button"));

            saveButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(initialMaxPoints + 1, dbPoll.MaxPoints);
            Assert.AreEqual(initialMaxPerVote - 1, dbPoll.MaxPerVote);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManagePollType_SaveOfNonPointsTypeAfterConfigChanges_DoesNotSavePollConfigChanges()
        {
            int? initialMaxPoints = _defaultPoll.MaxPoints;
            int? initialMaxPerVote = _defaultPoll.MaxPerVote;

            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement pointsVoteButton = _driver.FindElement(By.Id("points-vote-button"));
            IWebElement multiVoteButton = _driver.FindElement(By.Id("multi-vote-button"));

            IWebElement pointsPerPollControl = _driver.FindElement(By.Id("points-per-poll"));
            IWebElement pointsPerPollIncreaseButton = pointsPerPollControl.FindElement(By.Id("increase-button"));

            IWebElement pointsPerVoteControl = _driver.FindElement(By.Id("points-per-vote"));
            IWebElement pointsPerVoteDecreaseButton = pointsPerVoteControl.FindElement(By.Id("decrease-button"));

            pointsVoteButton.Click();
            pointsPerPollIncreaseButton.Click();
            pointsPerVoteDecreaseButton.Click();
            multiVoteButton.Click();

            IWebElement saveButton = _driver.FindElement(By.Id("save-button"));

            saveButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(initialMaxPoints, dbPoll.MaxPoints);
            Assert.AreEqual(initialMaxPerVote, dbPoll.MaxPerVote);
            Assert.AreEqual(PollType.Multi, dbPoll.PollType);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManagePollType_Save_WarnsUser()
        {
            Choice pollChoice = new Choice() { PollChoiceNumber = 1, Name = "Choice" };
            _defaultPoll.Choices.Add(pollChoice);

            Ballot pollBallot = new Ballot { ManageGuid = Guid.NewGuid(), TokenGuid = Guid.NewGuid(), Votes = new List<Vote>() };
            Vote pollVote = new Vote() { Poll = _defaultPoll, Ballot = pollBallot, Choice = pollChoice };
            pollBallot.Votes.Add(pollVote);

            _defaultPoll.Ballots.Add(pollBallot);

            _context.SaveChanges();

            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement multiVoteButton = _driver.FindElement(By.Id("multi-vote-button"));

            multiVoteButton.Click();

            IWebElement saveButton = _driver.FindElement(By.Id("save-button"));

            saveButton.Click();

            Assert.AreEqual(PollUrl, _driver.Url);

            IWebElement dialogContent = _driver.FindElement(By.ClassName("dialog-content"));
            Assert.IsTrue(dialogContent.IsVisible());
        }
    }
}
