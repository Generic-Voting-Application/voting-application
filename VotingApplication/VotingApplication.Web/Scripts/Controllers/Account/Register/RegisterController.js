(function () {
    'use strict';

    angular
        .module('VoteOn-Account')
        .controller('RegisterController', RegisterController);

    RegisterController.$inject = ['$scope'];

    function RegisterController($scope) {

        $scope.user = {
            email: null,
            password: null
        };

        $scope.register = register;

        function register() {
        }
    }
})();
