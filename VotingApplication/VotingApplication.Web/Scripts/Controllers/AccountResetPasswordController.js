/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ErrorService.js" />
(function () {
    "use strict";

    angular
        .module('GVA.Common')
        .controller('AccountResetPasswordController', AccountResetPasswordController);

    AccountResetPasswordController.$inject = ['$scope', '$routeParams', '$location', 'AccountService', 'ErrorService'];

    function AccountResetPasswordController($scope, $routeParams, $location, AccountService, ErrorService) {

        var emailParameter = $routeParams.email;
        var codeParameter = $routeParams.code;

        $scope.email = emailParameter;

        $scope.resetPassword = resetPassword;

        function resetPassword(form) {

            AccountService.resetPassword(emailParameter, codeParameter, form.password, form.confirmpassword).success(function () {
                $location.path('/');
            }).error(function (data, status) {
                if (status === 400 && data.ModelState) {
                    ErrorService.bindModelStateToForm(data.ModelState, form, displayError);
                }
            });
        }

        function displayError(errorMessage) {
            $scope.errorMessage = errorMessage;
        }
    }

})();
