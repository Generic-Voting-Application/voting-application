(function () {
    'use strict';

    angular
        .module('VoteOn-Account')
        .controller('RegisterController', RegisterController);

    RegisterController.$inject = ['$scope', '$window', 'AccountService'];

    function RegisterController($scope, $window, AccountService) {

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
                    .then(function () {
                        $window.location.href = '/Register/#/RegistrationComplete/' + userEmail;
                    });
            }
        }
    }
})();
