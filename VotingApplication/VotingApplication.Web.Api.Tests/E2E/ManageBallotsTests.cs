using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
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
        private static readonly string PollUrl = SiteBaseUri + "Dashboard/#/Manage/" + PollManageGuid;

        private ITestVotingContext _context;
        private Poll _defaultPoll;
        private IWebDriver _driver;

        [TestInitialize]
        public virtual void TestInitialise()
        {
            _context = new TestVotingContext();

            // Open, Anonymous, No Option Adding, Shown Results
            _defaultPoll = new Poll()
            {
                UUID = PollGuid,
                ManageId = PollManageGuid,
                PollType = PollType.Basic,
                Name = "Test Poll",
                LastUpdated = DateTime.Now,
                CreatedDate = DateTime.Now,
                Options = new List<Option>(),
                InviteOnly = false,
                NamedVoting = false,
                OptionAdding = false,
                HiddenResults = false,
                MaxPerVote = 3,
                MaxPoints = 4,
                Ballots = new List<Ballot>()
            };

            _defaultPoll.Ballots.Add(new Ballot()
            {
                TokenGuid = Guid.NewGuid(),
                ManageGuid = Guid.NewGuid(),
                VoterName = "Voter 1",
                Votes = new List<Vote>()
            });

            _defaultPoll.Ballots.Add(new Ballot()
            {
                TokenGuid = Guid.NewGuid(),
                ManageGuid = Guid.NewGuid(),
                VoterName = "Voter 2",
                Votes = new List<Vote>()
            });

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
        public void ManageBallots_CancelButton_NavigatesToManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Voters");

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));
            cancelButton.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId, _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageBallots_CancelButton_DoesNotSaveChanges()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Voters");

            IWebElement clearAllButton = _driver.FindElement(By.Id("clear-all-button"));
            clearAllButton.Click();

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));
            cancelButton.Click();

            Poll dbPoll = _context.Polls.Local.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(_defaultPoll.Ballots.Count, dbPoll.Ballots.Count);
        }
    }
}
