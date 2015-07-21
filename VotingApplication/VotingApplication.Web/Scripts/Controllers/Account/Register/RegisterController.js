/// <reference path="../../../Services/AccountService.js" />
/// <reference path="../../../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Register')
        .controller('RegisterController', RegisterController);

    RegisterController.$inject = ['$scope', 'AccountService', 'RoutingService', 'ErrorService'];

    function RegisterController($scope, AccountService, RoutingService, ErrorService) {

        $scope.user = {
            email: null,
            password: null
        };

        $scope.sending = false;
        $scope.register = register;

        function register() {

            if ($scope.registerAccountForm.$valid) {
                var userEmail = $scope.registerAccountForm.email.$viewValue;
                var password = $scope.registerAccountForm.password.$viewValue;

                $scope.sending = true;

                AccountService.register(userEmail, password)
                    .then(function () { RoutingService.navigateToRegistrationCompletePage(userEmail); })
                    .catch(function (response) {
                        ErrorService.handleRegistrationError(response);
                        $scope.sending = false;
                    });
            }
        }
    }
})();
