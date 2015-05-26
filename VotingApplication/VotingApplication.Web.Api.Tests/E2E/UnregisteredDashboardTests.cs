using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace VotingApplication.Web.Api.Tests.E2E
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
                driver.Navigate().GoToUrl(SiteBaseUri);

                IWebElement questionBox = FindElementById(driver, "question");
                questionBox.SendKeys("?");

                IWebElement quickPollButton = FindElementById(driver, "quickpoll-button");

                Assert.IsTrue(quickPollButton.Enabled);
            }
        }
    }
}
