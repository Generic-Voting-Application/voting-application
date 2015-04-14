/// <reference path="../Services/AccountService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('AccountLoginController', AccountLoginController);

    AccountLoginController.$inject = ['$scope', '$rootScope', 'AccountService'];

    function AccountLoginController($scope, $rootScope, AccountService) {

        $scope.loginAccount = loginAccount;
        $scope.forgottenPassword = forgottenPassword;

        function loginAccount(form) {
            clearError();

            AccountService.login(form.email, form.password)
                .then(closeDialog)
                .catch(displayErrorMessage);
        }

        function displayErrorMessage() {
            displayError($rootScope.error.readableMessage);
            $rootScope.error = null;
        }

        function forgottenPassword(form) {
            clearError();

            if (form.email === undefined) {
                displayError('Please supply email address.');
            }
            else {
                AccountService.forgotPassword(form.email)
                    .then(closeDialog)
                    .catch(function (data) { displayError(data.Message || data.error_description); });
            }
        }

        function closeDialog() {
            $scope.closeThisDialog();
        }

        function clearError() {
            $scope.displayError = null;
        }

        function displayError(errorMessage) {
            $scope.displayError = errorMessage;
        }


    }

})();
