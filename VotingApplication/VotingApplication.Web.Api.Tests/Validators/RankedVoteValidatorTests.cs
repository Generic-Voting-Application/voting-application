using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using VotingApplication.Data.Model;
using VotingApplication.Web.Api.Models.DBViewModels;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web.Tests.Validators
{
    [TestClass]
    public class BasicVoteValidatorTests
    {
        private IVoteValidator _validator;
        private Poll _poll;
        private ModelStateDictionary _modelState;

        [TestInitialize]
        public void Setup()
        {
            _validator = new RankedVoteValidator();
            _poll = new Poll();
            _modelState = new ModelStateDictionary();
        }

        [TestMethod]
        public void ValidateAcceptsOutOfOrderVotes()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 2, ChoiceId = 2 },
                                                                        new VoteRequestModel { VoteValue = 1, ChoiceId = 1 } };
            
            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsTrue(_modelState.IsValid);
        }

        [TestMethod]
        public void ValidateRejectsMultipleVotes()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 1, ChoiceId = 1 }, 
                                                                        new VoteRequestModel { VoteValue = 1, ChoiceId = 1 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsFalse(_modelState.IsValid);
            Assert.IsNotNull(_modelState["Vote"]);
            Assert.AreEqual(1, _modelState["Vote"].Errors.Count);
        }

        [TestMethod]
        public void ValidateRejectsVotesOutOfSequence()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 1, ChoiceId = 1 }, 
                                                                        new VoteRequestModel { VoteValue = 3, ChoiceId = 2 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsFalse(_modelState.IsValid);
            Assert.IsNotNull(_modelState["VoteValue"]);
            Assert.AreEqual(1, _modelState["VoteValue"].Errors.Count);
        }

        [TestMethod]
        public void ValidateRejectsVotesWithValueLessThanOrEqualToZero()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 0, ChoiceId = 1 } };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsFalse(_modelState.IsValid);
            Assert.IsNotNull(_modelState["VoteValue"]);
            Assert.AreEqual(1, _modelState["VoteValue"].Errors.Count);
        }

        [TestMethod]
        public void ValidateAcceptsValidInput()
        {
            // Arrange
            List<VoteRequestModel> votes = new List<VoteRequestModel> { new VoteRequestModel { VoteValue = 1, ChoiceId = 1},
                                                                        new VoteRequestModel { VoteValue = 2, ChoiceId = 2} };

            // Act
            _validator.Validate(votes, _poll, _modelState);

            // Assert
            Assert.IsTrue(_modelState.IsValid);
        }
    }
}
