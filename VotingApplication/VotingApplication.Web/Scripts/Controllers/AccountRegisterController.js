/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ErrorService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('AccountRegisterController', AccountRegisterController);

    AccountRegisterController.$inject = ['$scope', '$route', '$rootScope', 'AccountService'];

    function AccountRegisterController($scope, $route, $rootScope, AccountService) {

        $scope.registerAccountAndLogin = registerAccount;

        function registerAccount(form) {

            AccountService.registerAccountAndLogin(form.email, form.password)
                .then(function () {
                    closeDialog();
                    $route.reload();
                })
                .catch(displayErrorMessage);

            function closeDialog() {
                $scope.closeThisDialog();
            }

            function displayErrorMessage() {
                $scope.displayError = $rootScope.error.readableMessage;
                $rootScope.error = null;
            }
        }
    }
})();
