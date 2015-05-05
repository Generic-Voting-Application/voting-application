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
    [TestClass]
    public class ManageTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";

        private IVotingContext _context;
        private Poll _defaultPoll;
        private IWebDriver _driver;

        [TestInitialize]
        public virtual void TestInitialise()
        {
            _context = new TestVotingContext();

            // Open, Anonymous, No Option Adding, Shown Results
            _defaultPoll = new Poll()
            {
                UUID = Guid.NewGuid(),
                ManageId = Guid.NewGuid(),
                PollType = PollType.Basic,
                Name = "Test Poll",
                LastUpdated = DateTime.Now,
                CreatedDate = DateTime.Now,
                Options = new List<Option>(),
                InviteOnly = false,
                NamedVoting = false,
                OptionAdding = false,
                HiddenResults = false
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
        public void ManageNameChange_NameSection_DisplaysCurrentName()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageNameSection = _driver.FindElement(By.Id("manage-name-section"));

            IWebElement pollName = manageNameSection.FindElement(NgBy.Binding("Question"));

            Assert.IsTrue(pollName.IsVisible());
            Assert.AreEqual(_defaultPoll.Name, pollName.Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNameChange_NameUpdate_UpdatesName()
        {
            String newPollName = "Poll Name Updated";

            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageNameSection = _driver.FindElement(By.Id("manage-name-section"));

            IWebElement editButton = manageNameSection.FindElements(By.TagName("a")).Single();


            editButton.Click();

            IWebElement editInput = manageNameSection.FindElements(By.TagName("Input")).Single();
            editInput.Clear();
            editInput.SendKeys(newPollName);
            manageNameSection.FindElement(By.TagName("Form")).Submit();

            IWebElement pollName = manageNameSection.FindElement(NgBy.Binding("Question"));

            Assert.IsTrue(pollName.IsVisible());
            Assert.AreEqual(newPollName, pollName.Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_OptionSectionEditButton_NavigatesToOptionManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-options-section"));

            IReadOnlyCollection<IWebElement> links = manageSection.FindElements(By.TagName("a"));
            IWebElement editLink = links.First(l => l.Text == "Edit");

            editLink.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_PollTypeSectionEditButton_NavigatesToPollTypeManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-poll-type-section"));

            IReadOnlyCollection<IWebElement> links = manageSection.FindElements(By.TagName("a"));
            IWebElement editLink = links.First(l => l.Text == "Edit");

            editLink.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/PollType", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_MiscSectionEditButton_NavigatesToMiscManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-misc-section"));

            IReadOnlyCollection<IWebElement> links = manageSection.FindElements(By.TagName("a"));
            IWebElement editLink = links.First(l => l.Text == "Edit");

            editLink.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Misc", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_ExpirySectionEditButton_NavigatesToExpiryManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-expiry-section"));

            IReadOnlyCollection<IWebElement> links = manageSection.FindElements(By.TagName("a"));
            IWebElement editLink = links.First(l => l.Text == "Edit");

            editLink.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Expiry", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_InviteesSectionEditButton_NavigatesToInviteesManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-invitees-section"));

            IReadOnlyCollection<IWebElement> links = manageSection.FindElements(By.TagName("a"));
            IWebElement editLink = links.First(l => l.Text == "Edit");

            editLink.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Invitees", _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageNavigation_VotersSectionEditButton_NavigatesToVotersManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId);
            IWebElement manageSection = _driver.FindElement(By.Id("manage-voters-section"));

            IReadOnlyCollection<IWebElement> links = manageSection.FindElements(By.TagName("a"));
            IWebElement editLink = links.First(l => l.Text == "Edit");

            editLink.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Voters", _driver.Url);
        }
    }
}
