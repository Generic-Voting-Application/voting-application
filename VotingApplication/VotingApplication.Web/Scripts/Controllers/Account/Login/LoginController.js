/// <reference path="../../../Services/AccountService.js" />
/// <reference path="../../../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Login')
        .controller('LoginController', LoginController);

    LoginController.$inject = ['$scope', 'AccountService', 'RoutingService', 'ErrorService'];

    function LoginController($scope, AccountService, RoutingService, ErrorService) {

        $scope.user = {
            email: null,
            password: null
        };

        $scope.sending = false;
        $scope.login = login;
        $scope.forgottenPassword = forgottenPassword;

        function login() {

            if ($scope.loginForm.$valid) {
                var userEmail = $scope.loginForm.email.$viewValue;
                var password = $scope.loginForm.password.$viewValue;

                $scope.sending = true;

                AccountService.login(userEmail, password)
                    .then(RoutingService.returnToReferrerPage)
                    .catch(function (response) {
                        ErrorService.handleLoginError(response, userEmail);
                        $scope.sending = false;
                    });
            }
        }

        function forgottenPassword() {
            RoutingService.navigateToForgottenPasswordPage();
        }
    }
})();
