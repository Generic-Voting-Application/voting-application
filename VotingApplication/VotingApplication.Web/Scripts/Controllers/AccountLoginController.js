/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ErrorService.js" />
(function () {
    angular
        .module('GVA.Common')
        .controller('AccountLoginController', AccountLoginController);

    AccountLoginController.$inject = ['$scope', 'AccountService', 'ErrorService'];

    function AccountLoginController($scope, AccountService, ErrorService) {

        $scope.loginAccount = loginAccount;
        $scope.forgottenPassword = forgottenPassword;
        
        function loginAccount(form) {
            clearError();

            // Can this not be turned into a promise?
            AccountService.getAccessToken(form.email, form.password).success(function (data) {
                loginCallback(form.email, data);
            })
            .error(function (data, status) {
                loginFailureCallback(form, data, status);
            });
        }
        
        function forgottenPassword(form) {
            clearError();
            
            if (form.email === undefined) {
                displayError('Please supply email address.');
                return;
            }

            AccountService.forgotPassword(form.email).success(function (data) {
                console.log('Success');
            })
            .error(function (data, status) {
                console.log('Failed', data, status);
            });
        }

        function loginCallback(email, data) {
            AccountService.setAccount(data.access_token, email);

            $scope.closeThisDialog();
            if ($scope.ngDialogData.callback) {
                $scope.ngDialogData.callback();
            }
        }

        function loginFailureCallback(form, data, status) {
            // Bad request
            if (status === 400 && data.ModelState) {
                ErrorService.bindModelStateToForm(data.ModelState, form, displayError);
            }
            else {
                displayError(data.Message || data.error_description);
            }
        }

        function displayError(errorMessage) {
            $scope.errorMessage = errorMessage;
        }

        function clearError(errorMessage) {
            $scope.errorMessage = errorMessage;
        }
    }

})();
