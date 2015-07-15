/// <reference path="../../../Services/AccountService.js" />
/// <reference path="../../../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Login')
        .controller('LoginController', LoginController);

    LoginController.$inject = ['$scope', 'AccountService', 'RoutingService'];

    function LoginController($scope, AccountService, RoutingService) {

        $scope.user = {
            email: null,
            password: null
        };

        $scope.login = login;
        $scope.forgottenPassword = forgottenPassword;

        function login() {

            if ($scope.loginForm.$valid) {
                var userEmail = $scope.loginForm.email.$viewValue;
                var password = $scope.loginForm.password.$viewValue;

                AccountService.login(userEmail, password)
                    .then(RoutingService.returnToReferrerPage);
            }
        }

        function forgottenPassword() {
            RoutingService.navigateToForgottenPasswordPage();
        }
    }
})();
