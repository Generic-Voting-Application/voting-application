using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VotingApplication.Web.Api.Tests
{
    [TestClass]
    public class TestTest
    {
        [TestMethod]
        public void Test_Pass()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Fail()
        {
            Assert.Fail();
        }
    }
}