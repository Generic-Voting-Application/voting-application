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

        function sendResetLink() {

            if ($scope.forgottenPasswordForm.$valid) {
                var userEmail = $scope.forgottenPasswordForm.email.$viewValue;

                AccountService.forgotPassword(userEmail);
            }
        }
    }
})();
