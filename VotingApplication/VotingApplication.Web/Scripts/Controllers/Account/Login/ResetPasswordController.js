/// <reference path="../../../Services/AccountService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Login')
        .controller('ResetPasswordController', ResetPasswordController);

    ResetPasswordController.$inject = ['$scope', '$location', 'AccountService', 'RoutingService'];

    function ResetPasswordController($scope, $location, AccountService, RoutingService) {

        $scope.user = {
            password: null
        };

        $scope.sendResetLink = sendResetLink;
        $scope.sending = false;

        var email = decodeURIComponent($location.search().email);
        var resetToken = decodeURIComponent($location.search().code);

        function sendResetLink(resetPasswordForm) {

            if (resetPasswordForm.$valid) {
                $scope.sending = true;

                var userPassword = resetPasswordForm.password.$viewValue;

                AccountService.resetPassword(email, userPassword, resetToken)
                .then(function () {
                    RoutingService.navigateToHomePage();
                })
                .finally(function () {
                    $scope.sending = false;
                });
            }
        }
    }
})();