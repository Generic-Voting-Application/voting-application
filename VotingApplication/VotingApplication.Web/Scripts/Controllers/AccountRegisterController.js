(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('AccountRegisterController', ['$scope', 'AccountService', 'ErrorService', function ($scope, AccountService, ErrorService) {

        var displayError = function (errorMessage) {
            $scope.errorMessage = errorMessage;
        }

        $scope.registerAccount = function (form) {
            AccountService.register(form.email, form.password, function () {

                AccountService.getAccessToken(form.email, form.password, function (data) {

                    AccountService.setAccount(data.access_token);

                    $scope.closeThisDialog();
                    if ($scope.ngDialogData.callback) $scope.ngDialogData.callback();

                }, function (data, status) {
                    // Handle sign in error

                    // Bad request
                    if (status === 400 && data.ModelState) {
                        ErrorService.bindModelStateToForm(data.ModelState, form, displayError);
                    }
                });
            }, function (data, status) {
                // Handle register error

                // Bad request
                if (status === 400 && data.ModelState) {
                    ErrorService.bindModelStateToForm(data.ModelState, form, displayError);
                }
            });
        }

    }]);

})();
