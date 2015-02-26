(function () {
    angular.module('GVA.Common').controller('AccountLoginController', ['$scope', 'AccountService', function ($scope, AccountService) {

        var displayError = function (errorMessage) {
            $scope.errorMessage = errorMessage;
        }

        $scope.loginAccount = function (form) {

            AccountService.getAccessToken(form.email, form.password, function (data) {
                AccountService.setAccount(data.access_token);

                $scope.closeThisDialog();
                if ($scope.ngDialogData.callback) $scope.ngDialogData.callback();

            }, function (data, status) {
                // Bad request
                if (status === 400 && data.ModelState) {
                    ErrorService.bindModelStateToForm(data.ModelState, form, displayError);
                } else {
                    displayError(data.Message || data.error_description);
                }
            });
        }

    }]);

})();
