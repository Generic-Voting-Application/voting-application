/// <reference path="../Services/RoutingService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('ConfirmStepController', ConfirmStepController);

    ConfirmStepController.$inject = ['$scope', 'PollService', 'RoutingService'];

    function ConfirmStepController($scope, PollService, RoutingService) {

        $scope.getNonEmptyChoices = getNonEmptyChoices;
        $scope.getValidInvitees = getValidInvitees;
        $scope.pollHasWarnings = pollHasWarnings;
        $scope.pollHasErrors = pollHasErrors;
        $scope.questionIsValid = questionIsValid;
        $scope.choicesAreValid = choicesAreValid;
        $scope.styleIsValid = styleIsValid;
        $scope.expiryIsValid = expiryIsValid;
        $scope.invitationsAreValid = invitationsAreValid;
        $scope.formatUtcPollExpiry = formatUtcPollExpiry;
        $scope.expiryDateIsInPast = expiryDateIsInPast;
        $scope.createPoll = createPoll;

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

            if (!$scope.newPoll.PollName) {
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

        function getValidInvitees() {
            return $scope.newPoll.Invitations.filter(function (invite) {
                // Maybe this needs to be pulled out of this controller
                var emailRegEx = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
                return emailRegEx.test(invite);
            });
        }

        function formatUtcPollExpiry() {

            var utcDate = getUtcPollExpiry();

            if (utcDate) {
                return moment(utcDate).format('YYYY/MM/DD - HH:mm');
            } else {
                return 'Invalid Date';
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

        function createPollModel() {
            var poll = {};

            poll.PollName = $scope.newPoll.PollName;
            poll.Choices = getNonEmptyChoices();
            poll.ChoiceAdding = $scope.newPoll.ChoiceAdding;
            poll.PollType = $scope.newPoll.PollType;
            poll.NamedVoting = $scope.newPoll.NamedVoting;
            poll.ElectionMode = $scope.newPoll.ElectionMode;
            poll.ExpiryDateUtc = getUtcPollExpiry();
            poll.Invitations = getValidInvitees();
            poll.InviteOnly = $scope.newPoll.InviteOnly;

            return poll;
        }

        function createPoll() {
            PollService.createPoll(createPollModel())
                .then(function (response) {
                    var responseData = response.data;
                    RoutingService.navigateToVotePage(responseData.UUID, responseData.CreatorBallot.TokenGuid);
                });
        }
    }
})();
