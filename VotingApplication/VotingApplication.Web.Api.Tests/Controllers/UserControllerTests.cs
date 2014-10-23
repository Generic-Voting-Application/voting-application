using FakeDbSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VotingApplication.Data;
using VotingApplication.Data.Context;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Controllers;
using System;

namespace VotingApplication.Web.Api.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        private UserController _controller;
        private User _bobUser;
        private User _joeUser;
        private User _billUser;
        private InMemoryDbSet<User> dummyUsers;

        [TestInitialize]
        public void setup()
        {
            _bobUser = new User { Id = 1, Name = "Bob" };
            _joeUser = new User { Id = 2, Name = "Joe" };
            _billUser = new User { Name = "Bill" };
            dummyUsers = new InMemoryDbSet<User>(true);
            dummyUsers.Add(_bobUser);
            dummyUsers.Add(_joeUser);

            var mockContextFactory = new Mock<IContextFactory>();
            var mockContext = new Mock<IVotingContext>();
            mockContextFactory.Setup(a => a.CreateContext()).Returns(mockContext.Object);
            mockContext.Setup(a => a.Users).Returns(dummyUsers);
            mockContext.Setup(a => a.SaveChanges()).Callback(this.SaveChanges);

            _controller = new UserController(mockContextFactory.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        private void SaveChanges()
        {
            for (int i = 0; i < dummyUsers.Local.Count; i++)
            {
                dummyUsers.Local[i].Id = (long)i+1;
            }
        }

        #region GET
        [TestMethod]
        public void GetReturnsAllUsers()
        {
            // Act
            var response = _controller.Get();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetReturnsNonNullUsers()
        {
            // Act
            var response = _controller.Get();
            List<User> responseUsers = ((ObjectContent)response.Content).Value as List<User>;

            // Assert
            Assert.IsNotNull(responseUsers);
        }

        [TestMethod]
        public void GetReturnsUsersFromTheDatabase()
        {
            // Act
            var response = _controller.Get();
            List<User> responseUsers = ((ObjectContent)response.Content).Value as List<User>;

            // Assert
            List<User> expectedUsers = new List<User>();
            expectedUsers.Add(_bobUser);
            expectedUsers.Add(_joeUser);
            Debug.WriteLine(responseUsers);
            CollectionAssert.AreEquivalent(expectedUsers, responseUsers);
        }

        [TestMethod]
        public void GetWithIdFindsUserWithId()
        {
            // Act
            var response = _controller.Get(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsUserWithIdFromTheDatabase()
        {
            // Act
            var response = _controller.Get(2);
            User responseUser = ((ObjectContent)response.Content).Value as User;

            // Assert
            Assert.AreEqual(_joeUser.Id, responseUser.Id);
            Assert.AreEqual(_joeUser.Name, responseUser.Name);
        }

        [TestMethod]
        public void GetWithIdReturnsErrorCode404ForUnknownUserID()
        {
            // Act
            var response = _controller.Get(3);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetWithIdReturnsNullUserForUnknownUserID()
        {
            // Act
            var response = _controller.Get(3);
            User responseUser = ((ObjectContent)response.Content).Value as User;

            // Assert
            Assert.IsNull(responseUser);
        } 
        #endregion

        #region PUT

        [TestMethod]
        public void PutIsNotAllowed()
        {
            // Act
            var response = _controller.Put();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PutByIdIsNotAllowed()
        {
            // Act
            var response = _controller.Put(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region POST

        [TestMethod]
        public void PostIsNotAllowed()
        {
            // Act
            var response = _controller.Post();

            // Assert
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public void PostReturnsOK()
        {
            // Act
            var response = _controller.Post(_billUser);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void PostNewUserAddsNewUserToUserList()
        {
            // Act
            var response = _controller.Post(_billUser);

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            expectedList.Add(_billUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PostNewUserAssignsNewUserAUniqueId()
        {
            // Act
            var response = _controller.Post(_billUser);

            // Assert
            Assert.AreEqual(3, dummyUsers.Local[2].Id);
        }


        [TestMethod]
        public void PostNewUserReturnsNewUserId()
        {
            // Act
            var response = _controller.Post(_billUser);
            long responseUserId = (long)((ObjectContent)response.Content).Value;

            // Assert
            Assert.AreEqual(3, responseUserId);
        }

        [TestMethod]
        public void PostNewUserWithExistingUsernameIsNotAllowed()
        {
            // Act
            var response = _controller.Post(new User() { Name = "Bob" });

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public void PostNewUserWithExistingUsernameDoesNotChangeUserList()
        {
            // Act
            var response = _controller.Post(new User() { Name = "Bob" });

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PostNewUserWithEmptyUsernameIsNotAllowed()
        {
            // Act
            var response = _controller.Post(new User() { Name = String.Empty });

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public void PostNewUserWithEmptyUsernameDoesNotChangeUserList()
        {
            // Act
            var response = _controller.Post(new User() { Name = String.Empty });

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
        }

        [TestMethod]
        public void PostNewUserWithMissingUsernameIsNotAllowed()
        {
            // Act
            var response = _controller.Post(new User());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public void PostNewUserWithMissingUsernameDoesNotChangeUserList()
        {
            // Act
            var response = _controller.Post(new User());

            // Assert
            List<User> expectedList = new List<User>();
            expectedList.Add(_bobUser);
            expectedList.Add(_joeUser);
            CollectionAssert.AreEquivalent(expectedList, dummyUsers.Local);
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
