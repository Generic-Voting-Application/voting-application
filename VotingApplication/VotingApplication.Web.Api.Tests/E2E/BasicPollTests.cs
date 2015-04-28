using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;
using System;
using System.Collections.Generic;
using System.Linq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Tests.E2E.Helpers.Clearers;

namespace VotingApplication.Web.Tests.E2E
{
    [TestClass]
    public class BasicPollTests
    {
        private static readonly string ChromeDriverDir = @"..\..\";
        private static readonly string SiteBaseUri = @"http://localhost:64205/";

        private const int truncatedTextLimit = 60;

        private static Poll _testPoll;
        private static IWebDriver _driver;
        private static IVotingContext _context;

        [ClassInitialize]
        public static void ClassInitialise(TestContext testContext)
        {
            ContextFactory contextFactory = new ContextFactory();
            _context = contextFactory.CreateTestContext();

            List<Option> testPollOptions = new List<Option>() {
                new Option(){ Name = "Test Option 1", Description = "Test Description 1" },
                new Option(){ Name = "Test Option 2", 
                              Description = "A very long test description 2 that should exceed the character limit for descriptions" }
            };

            _testPoll = new Poll()
            {
                UUID = Guid.NewGuid(),
                Name = "Test Poll",
                LastUpdated = DateTime.Now,
                CreatedDate = DateTime.Now,
                Options = testPollOptions
            };

            _context.Polls.Add(_testPoll);
            _context.SaveChanges();

            _driver = new NgWebDriver(new ChromeDriver(ChromeDriverDir));
            _driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
            _driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            PollClearer pollTearDown = new PollClearer(_context);
            pollTearDown.ClearPoll(_testPoll);

            _driver.Dispose();
            _context.Dispose();
        }

        [TestMethod]
        public void PopulatedOptions_DisplaysAllOptions()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _testPoll.UUID);
            IReadOnlyCollection<IWebElement> optionNames = _driver.FindElements(NgBy.Binding("option.Name"));

            Assert.AreEqual(_testPoll.Options.Count, optionNames.Count);
            CollectionAssert.AreEquivalent(_testPoll.Options.Select(o => o.Name).ToList(),
                                           optionNames.Select(o => o.Text).ToList());
        }

        [TestMethod]
        public void PopulatedOptions_DisplaysAllOptionDescriptions()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _testPoll.UUID);
            IReadOnlyCollection<IWebElement> optionDescriptions = _driver.FindElements(NgBy.Model("option.Description"));

            Assert.AreEqual(_testPoll.Options.Count, optionDescriptions.Count);
            List<String> expectedDescriptions = new List<string>();

            foreach (Option option in _testPoll.Options)
            {
                string truncatedDescription = option.Description
                                                      .Split(' ')
                                                      .Aggregate((prev, curr) => (curr.Length + prev.Length >= truncatedTextLimit) ?
                                                                                 prev : prev + " " + curr);

                if (truncatedDescription != option.Description)
                {
                    truncatedDescription += "... Show More";
                }
                expectedDescriptions.Add(truncatedDescription);
            }

            CollectionAssert.AreEquivalent(expectedDescriptions,
                                           optionDescriptions.Select(o => o.Text).ToList());
        }

        [TestMethod]
        public void VotingOnOption_NavigatesToResultsPage()
        {
            _driver.Navigate().GoToUrl(SiteBaseUri + "Poll/#/Vote/" + _testPoll.UUID);
            IReadOnlyCollection<IWebElement> buttons = _driver.FindElements(By.TagName("Button"));
            buttons.First(b => b.Text == "Vote").Click();

            Assert.IsTrue(_driver.Url.StartsWith(SiteBaseUri + "Poll/#/Results/" + _testPoll.UUID));

            VoteClearer voterClearer = new VoteClearer(_context);
            voterClearer.ClearLast();
        }


    }
}
