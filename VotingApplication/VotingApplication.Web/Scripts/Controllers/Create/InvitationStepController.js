(function () {
    'use strict';

    angular
        .module('VoteOn-Create')
        .controller('InvitationStepController', InvitationStepController);

    InvitationStepController.$inject = ['$scope'];

    function InvitationStepController($scope) {

        $scope.validateEmail = validateEmail;

        function validateEmail(email) {
            var emailRegEx = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
            return emailRegEx.test(email);
        }
    }
})();
