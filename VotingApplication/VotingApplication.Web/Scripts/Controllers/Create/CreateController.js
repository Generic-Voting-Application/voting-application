(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('CreateController', CreateController);

    CreateController.$inject = ['$scope', '$timeout', 'RoutingService', 'PollService'];

    function CreateController($scope, $timeout, RoutingService, PollService) {

        $scope.newPoll = createDefaultPoll();
        $scope.currentStep = 0;
        $scope.steps = [
            {
                label: 'Question',
                content: '/Create/QuestionStep',
                locked: false
            },
            {
                label: 'Style',
                content: '/Create/StyleStep',
                locked: false
            },
            {
                label: 'Expiry',
                content: '/Create/ExpiryStep',
                locked: false
            },
            {
                label: 'Invitations',
                content: '/Create/InvitationStep',
                locked: false
            },
            {
                label: 'Confirm',
                content: '/Create/ConfirmStep',
                locked: false
            }
        ];

        $scope.advanceToStep = advanceToStep;
        $scope.getNonEmptyChoices = getNonEmptyChoices;
        $scope.getValidInvitees = getValidInvitees;
        $scope.invitationsAreValid = invitationsAreValid;
        $scope.formatPollExpiry = formatPollExpiry;
        $scope.expiryDateIsInPast = expiryDateIsInPast;
        $scope.createPoll = createPoll;
        
        function advanceToStep(step) {
            var stepIndex = step - 1;
            for (var i = 0; i <= stepIndex; i++) {
                $scope.steps[i].locked = false;
            }

            // Wait for the digest
            $timeout(function () {
                $scope.currentStep = stepIndex;
            });
        }

        function createDefaultPoll() {
            return {
                Name: '',
                PollType: 'Basic',
                MaxPerVote: null,
                MaxPoints: null,
                Invitations: [],
                OptionAdding: false,
                ExpiryDate: null
            };
        }

        function invitationsAreValid() {
            if ($scope.newPoll.InviteOnly && $scope.newPoll.Invitations.length === 0) {
                return false;
            }

            return true;
        }

        function getNonEmptyChoices() {
            if (!$scope.newPoll.Choices) {
                return [];
            }
            return $scope.newPoll.Choices.filter(function (choice) {
                return choice.Name;
            });
        }

        function getValidInvitees() {
            if (!$scope.newPoll.Invitations) {
                return [];
            }

            return $scope.newPoll.Invitations.filter(function (invite) {
                // Maybe this needs to be pulled out of this controller
                var emailRegEx = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
                return emailRegEx.test(invite);
            });
        }

        function formatPollExpiry(formatString) {

            var Date = getPollExpiry();

            if (Date) {
                return moment(Date).format(formatString);
            } else {
                return 'Invalid Date';
            }
        }

        function getPollExpiry() {
            if (!$scope.newPoll.ExpiryDate) {
                return null;
            }

            return moment($scope.newPoll.ExpiryDate).toDate();
        }

        function getPollExpiryUtc() {
            var date = getPollExpiry();

            if (date) {
                return moment(date).utc().toDate();
            }

            return null;
        }

        function expiryDateIsInPast() {
            if (!$scope.newPoll.ExpiryDate) {
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
            poll.ExpiryDateUtc = getPollExpiryUtc();
            poll.Invitations = getValidInvitees();
            poll.InviteOnly = $scope.newPoll.InviteOnly;

            return poll;
        }

        function createPoll() {
            PollService.createPoll(createPollModel())
                .then(function (response) {
                    RoutingService.navigateToVotePage(response.UUID, response.CreatorBallot.TokenGuid);
                });
        }
    }
})();
