(function () {
    'use strict';

    angular
        .module('GVA.Manage')
        .controller('ConfirmRegistrationController', ConfirmRegistrationController);

    ConfirmRegistrationController.$inject = ['$scope', '$routeParams', 'AccountService'];

    function ConfirmRegistrationController($scope, $routeParams, AccountService) {

        var email = $routeParams['email'];

        $scope.resendConfirmation = resendConfirmation;

        function resendConfirmation() {
            AccountService.resendConfirmation(email);
        }
    }
})();
