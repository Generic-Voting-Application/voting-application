using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Tests.E2E.Helpers;
using VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers;

namespace VotingApplication.Web.Tests.E2E
{
    [TestClass]
    public class ManageExpiryTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
        private static readonly Guid PollGuid = Guid.NewGuid();
        private static readonly Guid PollManageGuid = Guid.NewGuid();
        private static readonly string PollUrl = SiteBaseUri + "Dashboard/#/Manage/" + PollManageGuid + "/Expiry";
        private static readonly DateTime DefaultTime = DateTime.UtcNow;

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
                ExpiryDateUtc = DefaultTime
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

            IWebElement hourAdjustButton = _driver.FindElement(By.Id("hour-adjust-button"));
            hourAdjustButton.Click();

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.UUID == _defaultPoll.UUID).Single();

            Assert.AreEqual(DefaultTime, dbPoll.ExpiryDateUtc);
        }
    }
}
