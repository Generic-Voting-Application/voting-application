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
    public class ManageTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
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
                IsElectionMode = false
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
        public void NameSection_DisplaysCurrentName()
        {
            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageNameSection = _driver.FindElement(By.Id("manage-name-section"));

            IWebElement pollName = manageNameSection.FindElement(NgBy.Binding("Question"));

            Assert.IsTrue(pollName.IsVisible());
            Assert.AreEqual(_defaultPoll.Name, pollName.Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void NameUpdate_UpdatesName()
        {
            String newPollName = "Poll Name Updated";

            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageNameSection = _driver.FindElement(By.Id("manage-name-section"));

            IWebElement editLink = manageNameSection.FindElement(By.Id("edit-link"));

            editLink.Click();

            IWebElement editInput = manageNameSection.FindElements(By.TagName("Input")).Single();
            editInput.Clear();
            editInput.SendKeys(newPollName);
            manageNameSection.FindElement(By.TagName("Form")).Submit();

            IWebElement pollName = manageNameSection.FindElement(NgBy.Binding("Question"));

            Assert.IsTrue(pollName.IsVisible());
            Assert.AreEqual(newPollName, pollName.Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_ChoiceSectionEditButton_NavigatesToChoiceManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-choices-section"));

            IWebElement editLink = manageSection.FindElement(By.Id("edit-link"));

            editLink.Click();

            Assert.AreEqual(PollUrl + "/Choices", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_PollTypeSectionEditButton_NavigatesToPollTypeManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-poll-type-section"));

            IWebElement editLink = manageSection.FindElement(By.Id("edit-link"));

            editLink.Click();

            Assert.AreEqual(PollUrl + "/PollType", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_MiscSectionEditButton_NavigatesToMiscManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-misc-section"));

            IWebElement editLink = manageSection.FindElement(By.Id("edit-link"));

            editLink.Click();

            Assert.AreEqual(PollUrl + "/Misc", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_ExpirySectionEditButton_NavigatesToExpiryManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-expiry-section"));

            IWebElement editLink = manageSection.FindElement(By.Id("edit-link"));

            editLink.Click();

            Assert.AreEqual(PollUrl + "/Expiry", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_InviteesSectionEditButton_NavigatesToInviteesManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-invitees-section"));

            IWebElement editLink = manageSection.FindElement(By.Id("edit-link"));

            editLink.Click();

            Assert.AreEqual(PollUrl + "/Invitees", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_VotersSectionEditButton_NavigatesToVotersManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-voters-section"));

            IWebElement editLink = manageSection.FindElement(By.Id("edit-link"));

            editLink.Click();

            Assert.AreEqual(PollUrl + "/Voters", _driver.Url);
        }
    }
}
