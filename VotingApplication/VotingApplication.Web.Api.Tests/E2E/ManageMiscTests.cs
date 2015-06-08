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
    public class ManageMiscTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
        private static readonly int WaitTime = 500;
        private static readonly Guid PollGuid = Guid.NewGuid();
        private static readonly Guid PollManageGuid = Guid.NewGuid();
        private static readonly string PollUrl = SiteBaseUri + "Dashboard/#/Manage/" + PollManageGuid + "/Misc";

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
        public void ManageMisc_CancelButton_NavigatesToManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId, _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageMisc_CancelButton_DoesNotSaveChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement inviteOnlySwitch = _driver.FindElement(By.Id("InviteOnly"));
            inviteOnlySwitch.Click();

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.IsFalse(dbPoll.InviteOnly);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageMisc_Save_SavesChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement inviteOnlySwitch = _driver.FindElement(By.Id("InviteOnly"));
            IWebElement namedVotingSwitch = _driver.FindElement(By.Id("NamedVoting"));
            IWebElement choiceAddingSwitch = _driver.FindElement(By.Id("ChoiceAdding"));
            IWebElement isElectionModeSwitch = _driver.FindElement(By.Id("IsElectionMode"));

            inviteOnlySwitch.Click();
            namedVotingSwitch.Click();
            choiceAddingSwitch.Click();
            isElectionModeSwitch.Click();

            IWebElement saveButton = _driver.FindElement(By.Id("save-button"));

            saveButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.IsTrue(dbPoll.InviteOnly);
            Assert.IsTrue(dbPoll.NamedVoting);
            Assert.IsTrue(dbPoll.ChoiceAdding);
            Assert.IsTrue(dbPoll.IsElectionMode);
        }
    }
}
