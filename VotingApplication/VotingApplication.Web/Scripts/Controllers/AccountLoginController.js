/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ErrorService.js" />
(function () {
    angular
        .module('GVA.Common')
        .controller('AccountLoginController', AccountLoginController);

    AccountLoginController.$inject = ['$scope', 'AccountService', 'ErrorService']

    function AccountLoginController($scope, AccountService, ErrorService) {

        $scope.loginAccount = function (form) {

            AccountService.getAccessToken(form.email, form.password, callback, failureCallback);

            function callback(data) {
                AccountService.setAccount(data.access_token, form.email);

                $scope.closeThisDialog();
                if ($scope.ngDialogData.callback) {
                    $scope.ngDialogData.callback();
                }
            };

            function failureCallback(data, status) {
                // Bad request
                if (status === 400 && data.ModelState) {
                    ErrorService.bindModelStateToForm(data.ModelState, form, displayError);
                }
                else {
                    displayError(data.Message || data.error_description);
                }
            };

            var displayError = function (errorMessage) {
                $scope.errorMessage = errorMessage;
            }
        }
    };

})();
