(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('CreateController', CreateController);

    CreateController.$inject = ['$scope', '$timeout'];

    function CreateController($scope, $timeout) {

        $scope.newPoll = {};
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

        activate();

        function activate() {
            $scope.newPoll = createDefaultPoll();
        }

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
                DoesExpire: false
            };
        }
    }
})();
