using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;

namespace VotingApplication.Web.Api.Tests
{
    [TestClass]
    public class OptionControllerTests
    {
        private OptionController _controller;
        private Option _burgerOption;
        private Option _pizzaOption;
        private InMemoryDbSet<Option> _dummyOptions;

        [TestInitialize]
        public void setup()
        {
            _burgerOption = new Option { Id = 1, Name = "Burger King" };
            _pizzaOption = new Option { Id = 2, Name = "Pizza Hut" };
            _dummyOptions = new InMemoryDbSet<Option>(true);
            _dummyOptions.Add(_burgerOption);
            _dummyOptions.Add(_pizzaOption);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Options).Returns(_dummyOptions);
            mockContext.Setup(a => a.SaveChanges()).Callback(this.SaveChanges);

            _controller = new OptionController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyOptions.Local.Count; i++)
            {
                _dummyOptions.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetReturnsAllOptions()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsNonNullOptions()
        {
            // Act
            var response = _controller.Get();
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;

            // Assert
            Assert.IsNotNull(responseOptions);
        }

        [TestMethod]
        public void GetReturnsOptionsFromTheDatabase()
        {
            // Act
            var response = _controller.Get();
            List<Option> responseOptions = ((ObjectContent)response.Content).Value as List<Option>;

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_burgerOption);
            expectedOptions.Add(_pizzaOption);
            CollectionAssert.AreEquivalent(expectedOptions, responseOptions);
        }

        [TestMethod]
        public void GetWithIdFindsOptionWithId()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsOptionWithIdFromTheDatabase()
        {
            // Act
            var response = _controller.Get(2);
            Option responseOption = ((ObjectContent)response.Content).Value as Option;

            // Assert
            Assert.AreEqual(_pizzaOption.Id, responseOption.Id);
            Assert.AreEqual(_pizzaOption.Name, responseOption.Name);
        }

        [TestMethod]
        public void GetWithIdReturnsErrorCode404ForUnknownOptionID()
        {
            // Act
            var response = _controller.Get(3);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsNullUserForUnknownOptionID()
        {
            // Act
            var response = _controller.Get(3);
            Option responseOption = ((ObjectContent)response.Content).Value as Option;

            // Assert
            Assert.IsNull(responseOption);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(new Option() {Name="Abc", Description="Abc", Info="Abc"});

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostWithoutNameIsRejected()
        {
            // Act
            var response = _controller.Post(new Option() { Description = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void PostWithEmptyNameIsRejected()
        {
            // Act
            var response = _controller.Post(new Option() { Name="", Description = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Option name must not be blank", error.Message);
        }


        [TestMethod]
        public void PostWithoutDescriptionIsAccepted()
        {
            // Act
            var response = _controller.Post(new Option() { Name = "Abc", Info = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostWithoutMoreInfoIsAccepted()
        {
            // Act
            var response = _controller.Post(new Option() { Name = "Abc", Description = "Abc" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostAddsOptionToOptionList()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
            _controller.Post(newOption);

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_burgerOption);
            expectedOptions.Add(_pizzaOption);
            expectedOptions.Add(newOption);
            CollectionAssert.AreEquivalent(expectedOptions, _dummyOptions.Local);
        }

        [TestMethod]
        public void PostReturnsIdOfNewOption()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
            var response = _controller.Post(newOption);

            // Assert
            long optionId = (long)((ObjectContent)response.Content).Value;
            Assert.AreEqual(3, optionId);
        }

        [TestMethod]
        public void PostSetsIdOfNewOption()
        {
            // Act
            Option newOption = new Option() { Name = "Bella Vista" };
             _controller.Post(newOption);

            // Assert
            Assert.AreEqual(3, newOption.Id);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Option());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void DeleteIsNotAllowed()
        {
            // Act
            var response = _controller.Delete();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdIsAllowed()
        {
            // Act
            var response = _controller.Delete(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void DeleteByIdRemovesOptionWithMatchingId()
        {
            // Act
            _controller.Delete(1);

            // Assert
            List<Option> expectedOptions = new List<Option>();
            expectedOptions.Add(_pizzaOption);
            CollectionAssert.AreEquivalent(expectedOptions, _dummyOptions.Local);
            // Note: We assume that related entities that foreign key into this option will be deleted by entity framework
        }

        [TestMethod]
        public void DeleteByIdIsAllowedIfNoOptionMatchesId()
        {
            // Act
            var response = _controller.Delete(99);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion
    }
}
