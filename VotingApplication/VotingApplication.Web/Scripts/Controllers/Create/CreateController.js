(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('CreateController', CreateController);

    CreateController.$inject = ['$scope', '$timeout', 'RoutingService', 'CreateService', 'TokenService'];

    function CreateController($scope, $timeout, RoutingService, CreateService, TokenService) {

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
        $scope.goToQuestionStep = goToQuestionStep;
        $scope.goToStyleStep = goToStyleStep;
        $scope.goToExpiryStep = goToExpiryStep;
        $scope.goToInvitationStep = goToInvitationStep;


        $scope.getNonEmptyChoices = getNonEmptyChoices;
        $scope.getValidInvitees = getValidInvitees;
        $scope.getInvalidInvitees = getInvalidInvitees;
        $scope.invitationsAreValid = invitationsAreValid;
        $scope.formatPollExpiry = formatPollExpiry;
        $scope.expiryDateIsInPast = expiryDateIsInPast;
        $scope.createPoll = createPoll;

        var emailRegEx = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;

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

        function goToStyleStep() {
            advanceToStep(2);
        }

        function goToQuestionStep() {
            advanceToStep(1);
        }

        function goToExpiryStep() {
            advanceToStep(3);
        }

        function goToInvitationStep() {
            advanceToStep(4);
        }

        function createDefaultPoll() {
            return {
                Name: '',
                PollType: 'Basic',
                MaxPerVote: null,
                MaxPoints: null,
                Invitations: [],
                ChoiceAdding: false,
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
                return emailRegEx.test(invite);
            });
        }

        function getInvalidInvitees() {
            if (!$scope.newPoll.Invitations) {
                return [];
            }

            return $scope.newPoll.Invitations.filter(function (invite) {
                return !emailRegEx.test(invite);
            });
        }

        function formatPollExpiry(formatString) {

            var date = getPollExpiry();

            if (date) {
                return moment(date).format(formatString);
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
            poll.ExpiryDateUtc = getPollExpiryUtc();
            poll.Invitations = getValidInvitees();
            poll.InviteOnly = $scope.newPoll.InviteOnly;
            poll.PointsPerVote = $scope.newPoll.PointsPerVote;
            poll.MaxPoints = $scope.newPoll.PollType === 'Points' ? $scope.newPoll.MaxPoints : null;
            poll.MaxPerVote = $scope.newPoll.PollType === 'Points' ? $scope.newPoll.MaxPerVote : null;

            return poll;
        }

        function createPoll() {
            CreateService.createPoll(createPollModel())
                .then(function (response) {
                    TokenService.setToken(response.UUID, response.CreatorBallot.TokenGuid)
                        .then(function () {
                            RoutingService.navigateToVotePage(response.UUID);
                        });
                });
        }
    }
})();
