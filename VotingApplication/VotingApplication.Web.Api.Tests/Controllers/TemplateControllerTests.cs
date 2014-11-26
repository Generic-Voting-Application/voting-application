using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers.API_Controllers;

namespace VotingApplication.Web.Api.Tests.Controllers
{
    [TestClass]
    public class TemplateControllerTests
    {
        private TemplateController _controller;
        private Template _colourTemplate;
        private Template _emptyTemplate;
        private InMemoryDbSet<Template> _dummyTemplates;

        [TestInitialize]
        public void setup()
        {
            Option redOption = new Option() { Name = "Red" };
            Option blueOption = new Option() { Name = "Blue" };
            Option greenOption = new Option() { Name = "Green" };

            _colourTemplate = new Template() { Id = 1, Name = "Colours", Options = new List<Option>() { redOption, greenOption, blueOption } };
            _emptyTemplate = new Template() { Id = 2, Name = "Empty Set", Options = new List<Option>() };

            _dummyTemplates = new InMemoryDbSet<Template>(true);
            _dummyTemplates.Add(_colourTemplate);
            _dummyTemplates.Add(_emptyTemplate);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Templates).Returns(_dummyTemplates);
            mockContext.Setup(a => a.SaveChanges()).Callback(SaveChanges);

            _controller = new TemplateController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < _dummyTemplates.Local.Count; i++)
            {
                _dummyTemplates.Local[i].Id = (long)i + 1;
            }
        }

        #region GET

        [TestMethod]
        public void GetIsAllowed()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsAllTemplates()
        {
            // Act
            var response = _controller.Get();

            // Assert
            List<Template> expectedTemplates = new List<Template>() { _colourTemplate, _emptyTemplate };
            List<Template> responseTemplates = ((ObjectContent)response.Content).Value as List<Template>;
            CollectionAssert.AreEquivalent(expectedTemplates, responseTemplates);
        }

        [TestMethod]
        public void GetByIdIsAllowed()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetByIdReturnsTemplateWithMatchingId()
        {
            // Act
            var response = _controller.Get(2);

            // Assert
            Template responseTemplate = ((ObjectContent)response.Content).Value as Template;
            Assert.AreEqual(_emptyTemplate, responseTemplate);
        }

        [TestMethod]
        public void GetNonexistentTemplateByIdIsNotFound()
        {
            // Act
            var response = _controller.Get(99);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Template 99 does not exist", error.Message);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsAllowed()
        {
            // Act
            var response = _controller.Post(new Template() { Name = "New Template", Options = new List<Option>() });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostRejectsTemplateWithoutAName()
        {
            // Act
            var response = _controller.Post(new Template() { Options = new List<Option>() });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Template does not have a name", error.Message);
        }

        [TestMethod]
        public void PostRejectsTemplateWithABlankName()
        {
            // Act
            var response = _controller.Post(new Template() { Name = "", Options = new List<Option>() });

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            HttpError error = ((ObjectContent)response.Content).Value as HttpError;
            Assert.AreEqual("Template does not have a name", error.Message);
        }

        [TestMethod]
        public void PostAcceptsTemplateWithoutAnOptionList()
        {
            // Act
            var response = _controller.Post(new Template() { Name = "New Template" });

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostTemplateWithoutAnOptionListDefaultsToEmptyList()
        {
            // Act
            Template newTemplate = new Template() { Name = "New Template" };
            var response = _controller.Post(newTemplate);

            // Assert
            CollectionAssert.AreEquivalent(new List<Option>(), newTemplate.Options);
        }

        [TestMethod]
        public void PostTemplateWithAnOptionListRetainsOptionList()
        {
            // Act
            List<Option> optionList = new List<Option>();
            Option purpleOption = new Option() { Name = "Purple" };
            optionList.Add(purpleOption);

            Template newTemplate = new Template() { Name = "New Template", Options = optionList };
            var response = _controller.Post(newTemplate);

            // Assert
            CollectionAssert.AreEquivalent(optionList, newTemplate.Options);
        }

        [TestMethod]
        public void PostWithValidTemplateReturnsNewTemplateId()
        {
            // Act
            Template newTemplate = new Template() { Name = "New Template" };
            var response = _controller.Post(newTemplate);

            // Assert
            long responseId = (long)((ObjectContent)response.Content).Value;
            Assert.AreEqual(3, responseId);
        }

        [TestMethod]
        public void PostWithValidTemplateAssignsNewTemplateId()
        {
            // Act
            Template newTemplate = new Template() { Name = "New Template" };
            var response = _controller.Post(newTemplate);

            // Assert
            Assert.AreEqual(3, newTemplate.Id);
        }

        [TestMethod]
        public void PostWithValidTemplateAddsToTemplates()
        {
            // Act
            Template newTemplate = new Template() { Name = "New Template" };
            var response = _controller.Post(newTemplate);

            // Assign
            List<Template> expectedSets = new List<Template>();
            expectedSets.Add(_colourTemplate);
            expectedSets.Add(_emptyTemplate);
            expectedSets.Add(newTemplate);
            CollectionAssert.AreEquivalent(expectedSets, _dummyTemplates.Local);
        }

        [TestMethod]
        public void PostByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Post(1, new Template());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put(new Template());

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1, new Template());

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
        public void DeleteByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Delete(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion
    }
}
