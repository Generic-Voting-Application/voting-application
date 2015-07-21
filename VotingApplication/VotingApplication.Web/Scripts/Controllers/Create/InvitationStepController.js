(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('InvitationStepController', InvitationStepController);

    InvitationStepController.$inject = ['$scope', '$timeout'];

    function InvitationStepController($scope, $timeout) {

        $scope.validateEmail = validateEmail;
        $scope.evaluatePastedInput = evaluatePastedInput;

        function validateEmail(email) {
            var emailRegEx = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
            return emailRegEx.test(email);
        }

        // Confusingly, we can't simply pass the contents of the ng-model as
        // this the contents are evaluated before the paste actually occurs
        // To fix this, we pass the angular object itself and then get the model
        function evaluatePastedInput(chip) {
            // Wait until next digest
            $timeout(function () {
                var chipInput = chip.chipInput;
                var emailRegEx = /(([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?))/ig;
                var match;

                while ((match = emailRegEx.exec(chipInput)) !== null) {
                    $scope.newPoll.Invitations.push(match[0]);
                }

                chip.chipInput = '';
            });
        }
    }
})();
