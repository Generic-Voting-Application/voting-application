(function () {
    'use strict';

    angular
        .module('VoteOn-Account')
        .controller('RegistrationCompleteController', RegistrationCompleteController);

    RegistrationCompleteController.$inject = ['$scope', '$routeParams', 'AccountService'];

    function RegistrationCompleteController($scope, $routeParams, AccountService) {

        $scope.emailAddress = $routeParams['emailAddress'];

        $scope.confirmationResent = false;
        $scope.isSending = false;

        $scope.resendConfirmation = resendConfirmation;
        

        function resendConfirmation() {
            $scope.isSending = true;

            AccountService.resendConfirmation($scope.emailAddress)
                .then(function () {
                    $scope.confirmationResent = true;
                })
                .finally(function () {
                    $scope.isSending = false;
                });
        }
    }
})();
