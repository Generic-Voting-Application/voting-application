/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ErrorService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .controller('AccountRegisterController', AccountRegisterController);

    AccountRegisterController.$inject = ['$scope', '$route', '$rootScope', 'AccountService', 'RoutingService'];

    function AccountRegisterController($scope, $route, $rootScope, AccountService, RoutingService) {

        $scope.register = registerAccount;

        function registerAccount(form) {

            AccountService.register(form.email, form.password)
                .then(function () {
                    closeDialog();
                    RoutingService.navigateToConfirmRegistration(form.email);
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
