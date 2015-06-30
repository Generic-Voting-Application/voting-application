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
        $scope.formatUtcPollExpiry = formatUtcPollExpiry;
        $scope.expiryDateIsInPast = expiryDateIsInPast;

        var startingDate = new Date();

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
            if (!$scope.newPoll.OptionAdding && getNonEmptyChoices().length === 0) {
                return false;
            }

            return true;
        }

        function styleIsValid() {
            return true;
        }

        function expiryIsValid() {
            if (!$scope.newPoll.DoesExpire) {
                return true;
            }

            if (expiryDateIsInPast()) {
                return false;
            }

            return true;
        }

        function invitationsAreValid() {
            if ($scope.newPoll.InviteOnly && $scope.newPoll.Invitations.length === 0) {
                return false;
            }

            return true;
        }

        function getNonEmptyChoices() {
            return $scope.newPoll.Choices.filter(function (choice) {
                return choice.Name;
            });
        }

        function formatUtcPollExpiry() {

            var utcDate = getUtcPollExpiry();

            if (utcDate) {
                return moment(utcDate).format('MMMM Do YYYY, h:mm a');
            } else {
                return "Invalid Date";
            }
        }

        function getUtcPollExpiry() {
            if (!$scope.newPoll.DoesExpire) {
                return null;
            }

            var date;

            if (!$scope.newPoll.ExpiryDate) {
                date = startingDate;
            } else {
                date = new Date($scope.newPoll.ExpiryDate);
            }

            return moment(date).utc().toDate();
        }

        function expiryDateIsInPast() {
            if (!$scope.newPoll.DoesExpire) {
                return false;
            }

            var date = new Date($scope.newPoll.ExpiryDate);
            return (moment(date).isBefore(moment()));
        }
    }
})();
