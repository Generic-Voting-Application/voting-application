(function () {
    'use strict';

    angular
        .module('GVA.Manage')
        .controller('RegistrationConfirmationController', RegistrationConfirmationController);

    RegistrationConfirmationController.$inject = ['$scope', '$routeParams', 'AccountService'];

    function RegistrationConfirmationController($scope, $routeParams, AccountService) {

        var email = $routeParams['email'];

        $scope.resendConfirmation = resendConfirmation;

        function resendConfirmation() {
            AccountService.resendConfirmation(email);
        }
    }
})();
