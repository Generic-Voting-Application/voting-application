using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using VotingApplication.Data.Model;
using VotingApplication.Web.Tests.E2E.Helpers;

namespace VotingApplication.Web.Tests.E2E
{
    [TestClass]
    public class UnregisteredDashboardTests : E2ETest
    {
        [TestMethod]
        [TestCategory("E2E")]
        public void QuickPoll_DisabledWhenQuestionHasNoText()
        {
            using (IWebDriver driver = Driver)
            {
                GoToBaseUri(driver);

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
                GoToBaseUri(driver);

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
                    GoToBaseUri(driver);

                    IWebElement questionBox = FindElementById(driver, "question");
                    questionBox.SendKeys("Does this work?");

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
        public void SignInButton_DisplaysSignInDialog()
        {
            using (IWebDriver driver = Driver)
            {
                GoToBaseUri(driver);

                IWebElement signInButton = FindElementById(driver, "signin-button");
                signInButton.Click();

                IWebElement loginDialog = FindElementById(driver, "login-dialog");

                Assert.IsTrue(loginDialog.Displayed);
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public void RegisterButton_DisplaysRegisterDialog()
        {
            using (IWebDriver driver = Driver)
            {
                GoToBaseUri(driver);

                IWebElement registerButton = FindElementById(driver, "register-button");
                registerButton.Click();

                IWebElement registerDialog = FindElementById(driver, "register-dialog");

                Assert.IsTrue(registerDialog.Displayed);
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public void SignIn_Successfull_ShowsRegisteredDashboard()
        {
            using (IWebDriver driver = Driver)
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

                IWebElement registeredDashboard = FindElementById(driver, "registered-dashboard");

                Assert.IsTrue(registeredDashboard.Displayed);
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public void Register_Successfull_ShowsRegisteredDashboard()
        {
            using (IWebDriver driver = Driver)
            {
                GoToBaseUri(driver);

                IWebElement signInButton = FindElementById(driver, "register-button");
                signInButton.Click();

                IWebElement emailInput = FindElementById(driver, "email");
                IWebElement passwordInput = FindElementById(driver, "password");
                IWebElement signInDialogButton = FindElementById(driver, "register-button-dialog");

                emailInput.SendKeys(NewUserEmail);
                passwordInput.SendKeys(NewUserPassword);
                signInDialogButton.Click();

                IWebElement registeredDashboard = FindElementById(driver, "registered-dashboard");

                Assert.IsTrue(registeredDashboard.Displayed);
            }
        }
    }
}
