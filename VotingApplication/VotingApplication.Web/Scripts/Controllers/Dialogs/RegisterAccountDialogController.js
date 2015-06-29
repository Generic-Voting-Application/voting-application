/// <reference path="../../Services/AccountService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .controller('RegisterAccountDialogController', RegisterAccountDialogController);

    RegisterAccountDialogController.$inject = ['$scope', '$rootScope', '$mdDialog', 'AccountService'];


    function RegisterAccountDialogController($scope, $rootScope, $mdDialog, AccountService) {

        $scope.user = {
            email: null,
            password: null
        };

        $scope.register = registerAccount;

        function registerAccount() {

            if ($scope.registerAccountForm.$valid) {

                var userEmail = $scope.registerAccountForm.email.$viewValue;
                var password = $scope.registerAccountForm.password.$viewValue;

                AccountService.register(userEmail, password)
                    .then(function() {
                        $mdDialog.hide();
                    })
                    .catch(clearRootErrorMessage);
            }

            function clearRootErrorMessage() {
                $rootScope.error = null;
            }
        }
    }
})();