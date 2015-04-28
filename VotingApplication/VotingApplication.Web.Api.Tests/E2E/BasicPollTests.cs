using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;

namespace VotingApplication.Web.Tests.E2E
{
    [TestClass]
    public class BasicPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";

        private static Poll _testPoll;
        private static IWebDriver _driver;

        [ClassInitialize]
        public static void ClassInitialise(TestContext testContext)
        {
            IContextFactory contextFactory = new ContextFactory();
            using (IVotingContext context = contextFactory.CreateContext())
            {
                _testPoll = new Poll()
                {
                    UUID = Guid.NewGuid(),
                    Name = "Test Poll"

                };

                context.Polls.Add(_testPoll);
                context.SaveChanges();
            }

            _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _driver.Dispose();
        }

        [TestMethod]
        public void Test1()
        {

        }
    }
}
