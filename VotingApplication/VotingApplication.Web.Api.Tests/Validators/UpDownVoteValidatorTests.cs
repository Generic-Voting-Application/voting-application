using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web.Tests.Validators
{
    [TestClass]
    public class UpDownVoteValidatorTests
    {
        private IVoteValidator _validator;
        private Poll _poll;
        private ModelStateDictionary _modelState;

        [TestInitialize]
        public void Setup()
        {
            _validator = new UpDownVoteValidator();
            _poll = new Poll();
            _poll.Choices.AddRange(new List<Choice>()
            {
                new Choice() { Id = 1 },
                new Choice() { Id = 2 }
            });
            _modelState = new ModelStateDictionary();
        }

        [TestMethod]
        public void ValidateRejectsVoteWithInvalidPositiveValue()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 6 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsFalse(_modelState.IsValid);
            Assert.IsNotNull(_modelState["VoteValue"]);
            Assert.AreEqual(1, _modelState["VoteValue"].Errors.Count);
        }

        [TestMethod]
        public void ValidateRejectsVoteWithInvalidNegativeValue()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = -6 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsFalse(_modelState.IsValid);
            Assert.IsNotNull(_modelState["VoteValue"]);
            Assert.AreEqual(1, _modelState["VoteValue"].Errors.Count);
        }

        [TestMethod]
        public void ValidateRejectsMultipleVotes()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 0 }, 
                                                                        new VoteRequestModel { VoteValue = 0 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsFalse(_modelState.IsValid);
            Assert.IsNotNull(_modelState["Vote"]);
            Assert.AreEqual(1, _modelState["Vote"].Errors.Count);
        }

        [TestMethod]
        public void ValidateAcceptsValidPositiveInput()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 1 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsTrue(_modelState.IsValid);
        }

        [TestMethod]
        public void ValidateAcceptsValidNegativeInput()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = -1 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsTrue(_modelState.IsValid);
        }

        [TestMethod]
        public void ValidateAcceptsMultipleValidInput()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { 
                new VoteRequestModel { ChoiceId = 1, VoteValue = 1 },
                new VoteRequestModel { ChoiceId = 2, VoteValue = -1 }
            };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsTrue(_modelState.IsValid);
        }

        [TestMethod]
        public void ValidateRejectsDuplicateChoiceInput()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { 
                new VoteRequestModel { ChoiceId = 1, VoteValue = 1 },
                new VoteRequestModel { ChoiceId = 1, VoteValue = -1 }
            };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsFalse(_modelState.IsValid);
        }
    }
}