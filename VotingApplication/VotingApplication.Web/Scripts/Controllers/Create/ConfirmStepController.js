/// <reference path="../Services/RoutingService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('ConfirmStepController', ConfirmStepController);

    ConfirmStepController.$inject = ['$scope'];

    function ConfirmStepController($scope) {

        $scope.pollHasWarnings = pollHasWarnings;
        $scope.pollHasErrors = pollHasErrors;
        $scope.questionIsValid = questionIsValid;
        $scope.choicesAreValid = choicesAreValid;
        $scope.styleIsValid = styleIsValid;
        $scope.expiryIsValid = expiryIsValid;
        
        function pollHasWarnings() {
            return !choicesAreValid() ||
                   !styleIsValid() ||
                   !expiryIsValid() ||
                   !$scope.invitationsAreValid();
        }

        function pollHasErrors() {
            return !questionIsValid();
        }

        function questionIsValid() {

            if (!$scope.newPoll.PollName) {
                return false;
            }

            return true;
        }

        function choicesAreValid() {
            if (!$scope.newPoll.ChoiceAdding && $scope.getNonEmptyChoices().length === 0) {
                return false;
            }

            return true;
        }

        function styleIsValid() {
            return true;
        }

        function expiryIsValid() {
            if (!$scope.newPoll.ExpiryDate) {
                return true;
            }

            if ($scope.expiryDateIsInPast()) {
                return false;
            }

            return true;
        }
    }
})();
