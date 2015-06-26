/// <reference path="../../Services/AccountService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .controller('AccountRegisterController', AccountRegisterController);

    AccountRegisterController.$inject = ['$scope', '$rootScope', '$mdDialog', 'AccountService'];


    function AccountRegisterController($scope, $rootScope, AccountService, $mdDialog) {

        $scope.user = {
            email: null,
            password: null
        };

        $scope.register = registerAccount;
        $scope.cancel = cancel;

        function registerAccount(form) {

            var userEmail = form.email.$viewValue;
            var password = form.password.$viewValue;

            AccountService.register(userEmail, password)
                .then(function () {
                    $mdDialog.hide(userEmail);
                })
                .catch(clearRootErrorMessage);
        }

        function clearRootErrorMessage() {
            $rootScope.error = null;
        }

        function cancel() {
            $mdDialog.cancel();
        }
    }
})();