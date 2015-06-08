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
    public class ManageChoicesTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";
        private static readonly int WaitTime = 500;
        private static readonly int DialogClearWaitTime = 1000;
        private static readonly Guid PollGuid = Guid.NewGuid();
        private static readonly Guid PollManageGuid = Guid.NewGuid();
        private static readonly string PollUrl = SiteBaseUri + "Dashboard/#/Manage/" + PollManageGuid + "/Choices";

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

            _defaultPoll.Choices.Add(
                new Choice()
                {
                    Name = "Test Choice 1",
                    Description = "Test Description 1",
                    PollChoiceNumber = 1,
                }
            );

            _defaultPoll.Choices.Add(
                new Choice()
                {
                    Name = "Test Choice 2",
                    Description = "Test Description 2",
                    PollChoiceNumber = 2,
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
        public void DisplaysChoiceNames()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));

            Assert.AreEqual(_defaultPoll.Choices.Count, choiceNames.Count);
            CollectionAssert.AreEquivalent(_defaultPoll.Choices.Select(o => o.Name).ToList(),
                                           choiceNames.Select(o => o.Text).ToList());
        }

        [TestMethod, TestCategory("E2E")]
        public void DisplaysChoiceDescriptions()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> choiceDescriptions = _driver.FindElements(NgBy.Binding("choice.Description"));

            Assert.AreEqual(_defaultPoll.Choices.Count, choiceDescriptions.Count);
            CollectionAssert.AreEquivalent(_defaultPoll.Choices.Select(o => o.Description).ToList(),
                                           choiceDescriptions.Select(o => o.Text).ToList());
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

            IWebElement addChoiceButton = _driver.FindElement(By.PartialLinkText("New Choice"));
            addChoiceButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
            formName.SendKeys("Test");

            IWebElement addAnotherCheckbox = _driver.FindElement(By.Id("add-another-checkbox"));
            addAnotherCheckbox.Click();

            IWebElement addButton = _driver.FindElement(By.Id("add-button"));
            addButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IWebElement cancelButton = _driver.FindElement(By.Id("cancel-button"));

            cancelButton.Click();

            Poll dbPoll = _context.Polls.Where(p => p.ManageId == _defaultPoll.ManageId).Single();

            Thread.Sleep(WaitTime);
            _context.ReloadEntity(dbPoll);

            Assert.AreEqual(_defaultPoll.Choices.Count, dbPoll.Choices.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void Save_SavesChanges()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addChoiceButton = _driver.FindElement(By.PartialLinkText("New Choice"));
            addChoiceButton.Click();

            string newChoiceName = "Test Name";
            string newChoiceDescription = "Test Description";

            IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
            formName.SendKeys(newChoiceName);

            IWebElement formDescription = _driver.FindElement(NgBy.Model("addChoiceForm.description"));
            formDescription.SendKeys(newChoiceDescription);

            IWebElement addAnotherCheckbox = _driver.FindElement(By.Id("add-another-checkbox"));
            addAnotherCheckbox.Click();

            IWebElement addButton = _driver.FindElement(By.Id("add-button"));
            addButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in choices"));
            IWebElement lastChoice = choices.Last();

            IWebElement lastChoiceName = lastChoice.FindElement(NgBy.Binding("choice.Name"));
            IWebElement lastChoiceDescription = lastChoice.FindElement(NgBy.Binding("choice.Description"));

            Assert.AreEqual(newChoiceName, lastChoiceName.Text);
            Assert.AreEqual(newChoiceDescription, lastChoiceDescription.Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void AddButton_AddsOption()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addChoiceButton = _driver.FindElement(By.PartialLinkText("New Choice"));
            addChoiceButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
            formName.SendKeys("Test");

            IWebElement addButton = _driver.FindElement(By.Id("add-button"));

            addButton.Click();

            IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in choices"));

            Assert.AreEqual(_defaultPoll.Choices.Count + 1, choices.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void AddButton_DoesNotCloseForm()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addChoiceButton = _driver.FindElement(By.PartialLinkText("New Choice"));
            addChoiceButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
            formName.SendKeys("Test");

            IWebElement addButton = _driver.FindElement(By.Id("add-button"));

            addButton.Click();

            formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));

            Assert.IsTrue(formName.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void AddButtonWithoutAddAnotherToggle_AddsOption()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addChoiceButton = _driver.FindElement(By.PartialLinkText("New Choice"));
            addChoiceButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
            formName.SendKeys("Test");

            IWebElement addAnotherCheckbox = _driver.FindElement(By.Id("add-another-checkbox"));
            addAnotherCheckbox.Click();

            IWebElement addButton = _driver.FindElement(By.Id("add-button"));
            addButton.Click();

            IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in choices"));

            Assert.AreEqual(_defaultPoll.Choices.Count + 1, choices.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void AddButtonWithoutAddAnotherToggle_ClosesForm()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addChoiceButton = _driver.FindElement(By.PartialLinkText("New Choice"));
            addChoiceButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));
            formName.SendKeys("Test");

            IWebElement addAnotherCheckbox = _driver.FindElement(By.Id("add-another-checkbox"));
            addAnotherCheckbox.Click();

            IWebElement addButton = _driver.FindElement(By.Id("add-button"));
            addButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));

            Assert.IsFalse(formName.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void AddChoiceForm_RequiresAName()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IWebElement addChoiceButton = _driver.FindElement(By.PartialLinkText("New Choice"));
            addChoiceButton.Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("addChoiceForm.name"));

            IWebElement addButton = _driver.FindElement(By.Id("add-button"));

            Assert.IsFalse(addButton.Enabled);
        }

        [TestMethod, TestCategory("E2E")]
        public void DeleteButton_RemovesChoice()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> deleteButtons = _driver.FindElements(By.ClassName("fa-trash-o"));
            deleteButtons.First().Click();

            IReadOnlyCollection<IWebElement> choices = _driver.FindElements(NgBy.Repeater("choice in choices"));

            Assert.AreEqual(_defaultPoll.Choices.Count - 1, choices.Count);
        }

        [TestMethod, TestCategory("E2E")]
        public void EditButton_AllowsRenaming()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> editButtons = _driver.FindElements(By.ClassName("fa-pencil"));
            editButtons.First().Click();

            IWebElement formName = _driver.FindElement(NgBy.Model("editChoiceForm.name"));
            IWebElement formDescription = _driver.FindElement(NgBy.Model("editChoiceForm.description"));

            Assert.IsTrue(formName.IsVisible());
            Assert.IsTrue(formDescription.IsVisible());
        }

        [TestMethod, TestCategory("E2E")]
        public void EditSubmit_ChangesChoiceDetails()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> editButtons = _driver.FindElements(By.ClassName("fa-pencil"));
            editButtons.First().Click();

            IWebElement form = _driver.FindElement(By.Name("editChoiceForm"));

            IWebElement formName = _driver.FindElement(NgBy.Model("editChoiceForm.name"));
            IWebElement formDescription = _driver.FindElement(NgBy.Model("editChoiceForm.description"));

            string newName = "Changed Name";
            string newDescription = "Changed Description";

            formName.Clear();
            formDescription.Clear();

            formName.SendKeys(newName);
            formDescription.SendKeys(newDescription);

            IWebElement saveButton = _driver.FindElement(By.Id("dialog-save-button"));

            saveButton.Click();

            Thread.Sleep(DialogClearWaitTime);

            IReadOnlyCollection<IWebElement> choiceNames = _driver.FindElements(NgBy.Binding("choice.Name"));
            IReadOnlyCollection<IWebElement> choiceDescriptions = _driver.FindElements(NgBy.Binding("choice.Description"));

            Assert.AreEqual(newName, choiceNames.First().Text);
            Assert.AreEqual(newDescription, choiceDescriptions.First().Text);
        }

        [TestMethod, TestCategory("E2E")]
        public void EditSubmit_DoesNotAllowBlankName()
        {
            _driver.Navigate().GoToUrl(PollUrl);

            IReadOnlyCollection<IWebElement> editButtons = _driver.FindElements(By.ClassName("fa-pencil"));
            editButtons.First().Click();

            IWebElement form = _driver.FindElement(By.Name("editChoiceForm"));

            IWebElement formName = _driver.FindElement(NgBy.Model("editChoiceForm.name"));
            formName.Clear();

            IWebElement saveButton = _driver.FindElement(By.Id("dialog-save-button"));

            Assert.IsFalse(saveButton.Enabled);
        }

    }
}
