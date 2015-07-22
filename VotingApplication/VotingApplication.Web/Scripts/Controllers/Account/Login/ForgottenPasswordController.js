/// <reference path="../../../Services/AccountService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Login')
        .controller('ForgottenPasswordController', ForgottenPasswordController);

    ForgottenPasswordController.$inject = ['$scope', 'AccountService'];

    function ForgottenPasswordController($scope, AccountService) {

        $scope.user = {
            email: null
        };

        $scope.sendResetLink = sendResetLink;
        $scope.sending = false;
        $scope.resetSent = false;

        function sendResetLink(forgottenPasswordForm) {

            if (forgottenPasswordForm.$valid) {
                $scope.sending = true;

                var userEmail = forgottenPasswordForm.email.$viewValue;

                AccountService.forgotPassword(userEmail)
                .then(function () {
                    $scope.sending = false;
                    $scope.resetSent = true;
                })
                .catch(function () {
                    $scope.resetSent = true;
                });
            }
        }
    }
})();
