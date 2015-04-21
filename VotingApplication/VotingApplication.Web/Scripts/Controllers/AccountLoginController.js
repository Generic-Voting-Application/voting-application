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
                .then(function () {
                    closeDialog();
                    window.location.reload();
                })
                .catch(displayErrorMessage);
        }

        function displayErrorMessage() {
            showError($rootScope.error.readableMessage);
            $rootScope.error = null;
        }

        function forgottenPassword(form) {
            clearError();

            if (form.email === undefined) {
                showError('Please supply email address.');
            }
            else {
                AccountService.forgotPassword(form.email)
                    .then(closeDialog)
                    .catch(function (data) { showError(data.Message || data.error_description); });
            }
        }

        function closeDialog() {
            $scope.closeThisDialog();
        }

        function clearError() {
            $scope.displayError = null;
        }

        function showError(errorMessage) {
            $scope.displayError = errorMessage;
        }


    }

})();
