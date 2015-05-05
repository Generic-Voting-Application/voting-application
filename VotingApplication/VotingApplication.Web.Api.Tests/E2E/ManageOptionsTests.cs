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
using VotingApplication.Web.Api.Tests.E2E.Helpers;
using VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers;

namespace VotingApplication.Web.Tests.E2E
{
    [TestClass]
    public class ManageOptionsTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
        private static readonly int WaitTime = 500;
        private static readonly int DialogClearWaitTime = 1000;

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
                HiddenResults = false,
                MaxPerVote = 3,
                MaxPoints = 4
            };

            _defaultPoll.Options.Add(
                new Option()
                {
                    Name = "Test Option 1",
                    PollOptionNumber = 1,
                }
            );

            _defaultPoll.Options.Add(
                new Option()
                {
                    Name = "Test Option 2",
                    PollOptionNumber = 2,
                }
            );

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
        public void ManageOptions_CancelButton_NavigatesToManagement()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement cancelButton = buttons.First(l => l.Text == "Cancel");

            cancelButton.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId, _driver.Url);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_CancelButton_DoesNotSaveChanges()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement doneButton = buttons.First(l => l.Text == "Done");

            doneButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IWebElement cancelButton = buttons.First(l => l.Text == "Cancel");

            cancelButton.Click();

            Poll dbPoll = _context.Polls.Local.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(_defaultPoll.Options.Count, dbPoll.Options.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_Save_SavesChanges()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            string newOptionName = "Test Name";
            string newOptionDescription = "Test Description";

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys(newOptionName);

            IWebElement formDescription = _driver.FindElement(NgBy.Model("addOptionForm.description"));
            formDescription.SendKeys(newOptionDescription);

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement doneButton = buttons.First(l => l.Text == "Done");

            doneButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in options"));
            IWebElement lastOption = options.Last();

            IWebElement lastOptionName = lastOption.FindElement(NgBy.Binding("option.Name"));
            IWebElement lastOptionDescription = lastOption.FindElement(NgBy.Binding("option.Description"));

            Assert.AreEqual(newOptionName, lastOptionName.Text);
            Assert.AreEqual(newOptionDescription, lastOptionDescription.Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_AddAnotherButton_AddsOption()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement addAnotherButton = buttons.First(l => l.Text == "Add Another");

            addAnotherButton.Click();

            IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in options"));

            Assert.AreEqual(_defaultPoll.Options.Count + 1, options.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_AddAnotherButton_DoesNotCloseForm()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement addAnotherButton = buttons.First(l => l.Text == "Add Another");

            addAnotherButton.Click();

            formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

            Assert.IsTrue(formName.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_DoneButton_AddsOption()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement addAnotherButton = buttons.First(l => l.Text == "Done");

            addAnotherButton.Click();

            IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in options"));

            Assert.AreEqual(_defaultPoll.Options.Count + 1, options.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_DoneButton_ClosesForm()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement doneButton = buttons.First(l => l.Text == "Done");

            doneButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

            Assert.IsFalse(formName.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_AddOptionForm_RequiresAName()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId + "/Options");

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("button"));
            IWebElement doneButton = buttons.First(l => l.Text == "Done");
            IWebElement addAnotherButton = buttons.First(l => l.Text == "Add Another");

            Assert.IsFalse(doneButton.Enabled);
            Assert.IsFalse(addAnotherButton.Enabled);
        }
    }
}
