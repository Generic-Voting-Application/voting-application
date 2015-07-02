(function () {
    'use strict';

    angular
        .module('VoteOn-Login')
        .controller('LoginController', LoginController);

    LoginController.$inject = ['$scope', '$window', 'AccountService'];

    function LoginController($scope,$window, AccountService) {

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

                AccountService.login(userEmail, password);
            }
        }

        function forgottenPassword() {
            $window.location.href = '/Login/#/ForgottenPassword';
        }
    }
})();
