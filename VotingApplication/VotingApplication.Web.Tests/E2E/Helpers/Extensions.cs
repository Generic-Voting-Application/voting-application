using System;
using OpenQA.Selenium;

namespace VotingApplication.Web.Tests.E2E.Helpers
{
    public static class Extensions
    {
        public static bool IsVisible(this IWebElement element)
        {
            try
            {
                return element.Displayed;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
    }
}
