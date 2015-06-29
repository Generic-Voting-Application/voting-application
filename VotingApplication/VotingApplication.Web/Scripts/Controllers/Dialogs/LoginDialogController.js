(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .controller('LoginDialogController', LoginDialogController);

    LoginDialogController.$inject = ['$scope', '$rootScope', '$mdDialog', '$timeout', 'AccountService'];

    function LoginDialogController($scope, $rootScope, $mdDialog, $timeout, AccountService) {

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
                    .then(function () {
                        var loginSuccess = { signInSuccessful: true, forgottenPassword: false };

                        $mdDialog.hide(loginSuccess);
                    })
                    .catch(clearRootErrorMessage);
            }
        }

        function clearRootErrorMessage() {
            $rootScope.error = null;
        }

        function forgottenPassword() {
            var forgotPassword = { signInSuccessful: false, forgottenPassword: true };

            $timeout(function () { $mdDialog.hide(forgotPassword); });
        }
    }
})();
