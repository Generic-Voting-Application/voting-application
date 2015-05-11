using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web.Tests.Validators
{
    [TestClass]
    public class RankedVoteValidatorTests
    {
        private IVoteValidator _validator;
        private Poll _poll;
        private ModelStateDictionary _modelState;

        [TestInitialize]
        public void Setup()
        {
            _validator = new BasicVoteValidator();
            _poll = new Poll();
            _modelState = new ModelStateDictionary();
        }

        [TestMethod]
        public void ValidateRejectsVoteWithInvalidValue()
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
        public void ValidateAcceptsValidInput()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 1 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsTrue(_modelState.IsValid);
        }
    }
}