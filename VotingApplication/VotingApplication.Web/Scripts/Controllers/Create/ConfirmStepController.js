(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('ConfirmStepController', ConfirmStepController);

    ConfirmStepController.$inject = ['$scope'];

    function ConfirmStepController($scope) {

        $scope.getNonEmptyChoices = getNonEmptyChoices;
        $scope.pollHasWarnings = pollHasWarnings;
        $scope.pollHasErrors = pollHasErrors;
        $scope.questionIsValid = questionIsValid;
        $scope.choicesAreValid = choicesAreValid;
        $scope.styleIsValid = styleIsValid;
        $scope.expiryIsValid = expiryIsValid;
        $scope.invitationsAreValid = invitationsAreValid;

        activate();

        function activate() {

        }

        function pollHasWarnings() {
            return !choicesAreValid() ||
                   !styleIsValid() ||
                   !expiryIsValid() ||
                   !invitationsAreValid();
        }

        function pollHasErrors() {
            return !questionIsValid();
        }

        function questionIsValid() {

            if (!$scope.newPoll.Name) {
                return false;
                
            }

            return true;
        }

        function choicesAreValid() {
            if (!getNonEmptyChoices().length && !$scope.newPoll.OptionAdding) {
                return false;
            }

            return true;
        }

        function styleIsValid() {
            return true;
        }

        function expiryIsValid() {
            return true;
        }

        function invitationsAreValid() {
            if ($scope.newPoll.InviteOnly && !$scope.newPoll.Invitations.length) {
                return false;
            }

            return true;
        }

        function getNonEmptyChoices() {
            return $scope.newPoll.Choices.filter(function (choice) {
                return choice.Name;
            });
        }
    }
})();
