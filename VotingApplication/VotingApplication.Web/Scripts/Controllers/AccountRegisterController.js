/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ErrorService.js" />
(function () {
    "use strict";

    angular
        .module('GVA.Common')
        .controller('AccountRegisterController', AccountRegisterController);

    AccountRegisterController.$inject = ['$scope', 'AccountService', 'ErrorService'];

    function AccountRegisterController($scope, AccountService, ErrorService) {

        var displayError = function (errorMessage) {
            $scope.errorMessage = errorMessage;
        };

        $scope.registerAccount = function (form) {
            AccountService.register(form.email, form.password).success(function() {
                return AccountService.getAccessToken(form.email, form.password);
            }).success(function (data) {
                AccountService.setAccount(data.access_token, form.email);

                $scope.closeThisDialog();
                if ($scope.ngDialogData.callback) {
                    $scope.ngDialogData.callback();
                }
            }).error(function (data, status) {
                // Bad request
                if (status === 400 && data.ModelState) {
                    ErrorService.bindModelStateToForm(data.ModelState, form, displayError);
                } else {
                    displayError(data.Message || data.error_description);
                }
            });
        };

    }

})();
