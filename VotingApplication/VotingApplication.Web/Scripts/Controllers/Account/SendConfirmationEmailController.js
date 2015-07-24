(function () {
    'use strict';

    angular
        .module('VoteOn-Account')
        .controller('SendConfirmationEmailController', SendConfirmationEmailController);

    SendConfirmationEmailController.$inject = ['$scope', '$routeParams', 'AccountService'];

    function SendConfirmationEmailController($scope, $routeParams, AccountService) {

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
