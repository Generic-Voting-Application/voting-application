using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using VotingApplication.Data.Model;
using VotingApplication.Web.Tests.E2E.Helpers;

namespace VotingApplication.Web.Tests.E2E
{
    [TestClass]
    public class RegisteredDashboardTests : E2ETest
    {
        private static readonly Guid CreatedPollManageId = new Guid("FB74707A-C9D5-4B90-9F0C-D3280041B56C");


        [TestMethod]
        [TestCategory("E2E")]
        public void QuickPoll_DisabledWhenQuestionHasNoText()
        {
            using (IWebDriver driver = Driver)
            {
                SignIn(driver);

                IWebElement questionBox = FindElementById(driver, "question");
                questionBox.Clear();

                IWebElement quickPollButton = FindElementById(driver, "quickpoll-button");

                Assert.IsFalse(quickPollButton.Enabled);
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public void QuickPoll_EnabledWhenQuestionHasText()
        {
            using (IWebDriver driver = Driver)
            {
                SignIn(driver);

                IWebElement questionBox = FindElementById(driver, "question");
                questionBox.SendKeys("?");

                IWebElement quickPollButton = FindElementById(driver, "quickpoll-button");

                Assert.IsTrue(quickPollButton.Enabled);
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public void QuickPoll_NavigatesToManagePollPage()
        {
            using (IWebDriver driver = Driver)
            {
                using (var context = new TestVotingContext())
                {
                    SignIn(driver);

                    IWebElement questionBox = FindElementById(driver, "question");
                    questionBox.SendKeys("Does this work?");

                    //WaitForPageChange();

                    IWebElement quickPollButton = FindElementById(driver, "quickpoll-button");
                    quickPollButton.Click();

                    WaitForPageChange();

                    Poll newPoll = context.Polls.Single();

                    string expectedUrl = SiteBaseUri + "Dashboard/#/Manage/" + newPoll.ManageId;

                    Assert.AreEqual(expectedUrl, driver.Url);
                }
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public void ExistingPoll_Manage_NavigatesToManagePollPage()
        {
            using (IWebDriver driver = Driver)
            {
                using (var context = new TestVotingContext())
                {
                    CreatePoll(context);

                    SignIn(driver);

                    IWebElement managePollButton = driver.FindElement(By.ClassName("poll-panel-button"));
                    managePollButton.Click();

                    WaitForPageChange();

                    Poll newPoll = context.Polls.Single();

                    string expectedUrl = SiteBaseUri + "Dashboard/#/Manage/" + newPoll.ManageId;

                    Assert.AreEqual(expectedUrl, driver.Url);
                }
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public void ExistingPoll_Repeat_NavigatesToNewPollManagePollPage()
        {
            using (IWebDriver driver = Driver)
            {
                using (var context = new TestVotingContext())
                {
                    CreatePoll(context);

                    SignIn(driver);

                    ReadOnlyCollection<IWebElement> buttons = driver.FindElements(By.ClassName("poll-panel-button"));
                    Assert.AreEqual(2, buttons.Count);

                    IWebElement repeatButton = buttons[1];
                    repeatButton.Click();

                    WaitForPageChange();

                    Poll newPoll = context
                        .Polls
                        .Single(p => p.ManageId != CreatedPollManageId);

                    string expectedUrl = SiteBaseUri + "Dashboard/#/Manage/" + newPoll.ManageId;

                    Assert.AreEqual(expectedUrl, driver.Url);
                }
            }
        }

        private void SignIn(IWebDriver driver)
        {
            GoToBaseUri(driver);

            CreateNewUser();

            IWebElement signInButton = FindElementById(driver, "signin-button");
            signInButton.Click();

            IWebElement emailInput = FindElementById(driver, "email");
            IWebElement passwordInput = FindElementById(driver, "password");
            IWebElement signInDialogButton = FindElementById(driver, "signIn-button-dialog");

            emailInput.SendKeys(NewUserEmail);
            passwordInput.SendKeys(NewUserPassword);
            signInDialogButton.Click();

            WaitForPageChange();
        }

        private static void CreatePoll(TestVotingContext dbContext)
        {
            Poll poll = new Poll
            {
                UUID = Guid.NewGuid(),
                CreatorIdentity = NewUserId,
                ManageId = CreatedPollManageId,
                Name = "Test",
                LastUpdatedUtc = DateTime.UtcNow,
                CreatedDateUtc = DateTime.UtcNow
            };

            dbContext.Polls.Add(poll);

            dbContext.SaveChanges();
        }
    }
}
