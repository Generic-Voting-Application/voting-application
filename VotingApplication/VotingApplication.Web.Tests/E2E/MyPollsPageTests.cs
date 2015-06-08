using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using VotingApplication.Data.Model;
using VotingApplication.Web.Tests.E2E.Helpers;

namespace VotingApplication.Web.Tests.E2E
{
    [TestClass]
    public class MyPollsPageTests : E2ETest
    {
        private static readonly Guid CreatedPollManageId = new Guid("FB74707A-C9D5-4B90-9F0C-D3280041B56C");

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

                    NavigateToMyPolls(driver);

                    IWebElement managePollButton = driver.FindElement(By.ClassName("poll-panel-button"));
                    managePollButton.Click();

                    WaitForPageChange();

                    Poll newPoll = context.Polls.Single();

                    string expectedUrl = SiteBaseUri + "Manage/#/Manage/" + newPoll.ManageId;

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

                    NavigateToMyPolls(driver);

                    ReadOnlyCollection<IWebElement> buttons = driver.FindElements(By.ClassName("poll-panel-button"));
                    Assert.AreEqual(2, buttons.Count);

                    IWebElement repeatButton = buttons[1];
                    repeatButton.Click();

                    WaitForPageChange();

                    Poll newPoll = context
                        .Polls
                        .Single(p => p.ManageId != CreatedPollManageId);

                    string expectedUrl = SiteBaseUri + "Manage/#/Manage/" + newPoll.ManageId;

                    Assert.AreEqual(expectedUrl, driver.Url);
                }
            }
        }

        private void SignIn(IWebDriver driver)
        {
            GoToBaseUri(driver);

            CreateNewUser();

            IWebElement signInButton = FindElementById(driver, "signin-link");
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

        private static void NavigateToMyPolls(IWebDriver driver)
        {
            IWebElement accountControlToggle = FindElementById(driver, "account-control-toggle");
            accountControlToggle.Click();

            IWebElement myPollsLink = FindElementById(driver, "my-polls-link");
            myPollsLink.Click();
        }
    }
}
