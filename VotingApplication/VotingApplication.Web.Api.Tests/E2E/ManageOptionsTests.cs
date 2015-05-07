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
        private static readonly Guid PollGuid = Guid.NewGuid();
        private static readonly Guid PollManageGuid = Guid.NewGuid();
        private static readonly string PollUrl = SiteBaseUri + "Dashboard/#/Manage/" + PollManageGuid + "/Options";

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
                MaxPoints = 4
            };

            _defaultPoll.Options.Add(
                new Option()
                {
                    Name = "Test Option 1",
                    Description = "Test Description 1",
                    PollOptionNumber = 1,
                }
            );

            _defaultPoll.Options.Add(
                new Option()
                {
                    Name = "Test Option 2",
                    Description = "Test Description 2",
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
        public void ManageOptions_DisplaysOptionNames()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

            Assert.AreEqual(_defaultPoll.Options.Count, optionNames.Count);
            CollectionAssert.AreEquivalent(_defaultPoll.Options.Select(o => o.Name).ToList(),
                                           optionNames.Select(o => o.Text).ToList());
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_DisplaysOptionDescriptions()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Binding("option.Description"));

            Assert.AreEqual(_defaultPoll.Options.Count, optionDescriptions.Count);
            CollectionAssert.AreEquivalent(_defaultPoll.Options.Select(o => o.Description).ToList(),
                                           optionDescriptions.Select(o => o.Text).ToList());
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_CancelButton_NavigatesToManagement()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Assert.AreEqual(SiteBaseUri + "Dashboard/#/Manage/" + _defaultPoll.ManageId, _driver.Url);
        }


        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_CancelButton_DoesNotSaveChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IWebElement doneButton = _driver.FindElement(By.Id("done-button"));

            doneButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(_defaultPoll.Options.Count, dbPoll.Options.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_Save_SavesChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            string newOptionName = "Test Name";
            string newOptionDescription = "Test Description";

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys(newOptionName);

            IWebElement formDescription = _driver.FindElement(NgBy.Model("addOptionForm.description"));
            formDescription.SendKeys(newOptionDescription);

            IWebElement doneButton = _driver.FindElement(By.Id("done-button"));

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
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IWebElement addAnotherButton = _driver.FindElement(By.Id("add-another-button"));

            addAnotherButton.Click();

            IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in options"));

            Assert.AreEqual(_defaultPoll.Options.Count + 1, options.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_AddAnotherButton_DoesNotCloseForm()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IWebElement addAnotherButton = _driver.FindElement(By.Id("add-another-button"));

            addAnotherButton.Click();

            formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

            Assert.IsTrue(formName.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_DoneButton_AddsOption()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IWebElement doneButton = _driver.FindElement(By.Id("done-button"));

            doneButton.Click();

            IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in options"));

            Assert.AreEqual(_defaultPoll.Options.Count + 1, options.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_DoneButton_ClosesForm()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));
            formName.SendKeys("Test");

            IWebElement doneButton = _driver.FindElement(By.Id("done-button"));

            doneButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

            Assert.IsFalse(formName.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_AddOptionForm_RequiresAName()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addOptionButton = _driver.FindElement(By.PartialLinkText("New Option"));
            addOptionButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addOptionForm.name"));

            IWebElement doneButton = _driver.FindElement(By.Id("done-button"));
            IWebElement addAnotherButton = _driver.FindElement(By.Id("add-another-button"));

            Assert.IsFalse(doneButton.Enabled);
            Assert.IsFalse(addAnotherButton.Enabled);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_DeleteButton_RemovesOption()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> deleteButtons = _driver.FindElements(By.ClassName("fa-trash-o"));
            deleteButtons.First().Click();

            IReadOnlyCollection<IWebElement> options = _driver.FindElements(NgBy.Repeater("option in options"));

            Assert.AreEqual(_defaultPoll.Options.Count - 1, options.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_EditButton_AllowsRenaming()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> editButtons = _driver.FindElements(By.ClassName("fa-pencil"));
            editButtons.First().Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("editOptionFormName"));
            IWebElement formDescription = _driver.FindElement(NgBy.Model("editOptionFormDescription"));

            Assert.IsTrue(formName.IsVisible());
            Assert.IsTrue(formDescription.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_EditSubmit_ChangesOptionDetails()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> editButtons = _driver.FindElements(By.ClassName("fa-pencil"));
            editButtons.First().Click();

            IWebElement form = _driver.FindElement(By.Name("editOptionForm"));

            IWebElement formName = _driver.FindElement(NgBy.Model("editOptionFormName"));
            IWebElement formDescription = _driver.FindElement(NgBy.Model("editOptionFormDescription"));

            string newName = "Changed Name";
            string newDescription = "Changed Description";

            formName.Clear();
            formDescription.Clear();

            formName.SendKeys(newName);
            formDescription.SendKeys(newDescription);

            IWebElement saveButton = _driver.FindElement(By.Id("dialog-save-button"));

            saveButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));
            IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Binding("option.Description"));

            Assert.AreEqual(newName, optionNames.First().Text);
            Assert.AreEqual(newDescription, optionDescriptions.First().Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void ManageOptions_EditSubmit_DoesNotAllowBlankName()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> editButtons = _driver.FindElements(By.ClassName("fa-pencil"));
            editButtons.First().Click();

            IWebElement form = _driver.FindElement(By.Name("editOptionForm"));

            IWebElement formName = _driver.FindElement(NgBy.Model("editOptionFormName"));
            formName.Clear();

            IWebElement saveButton = _driver.FindElement(By.Id("dialog-save-button"));

            Assert.IsFalse(saveButton.Enabled);
        }

    }
}
