(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('AccountRegisterController', ['$scope', 'AccountService', function ($scope, AccountService) {

        $scope.registerAccount = function (form) {
            AccountService.register(form.email, form.password, function () {

                AccountService.getAccessToken(form.email, form.password, function (data) {

                    AccountService.setAccount(data.access_token);

                    $scope.closeThisDialog();
                    if ($scope.ngDialogData.callback) $scope.ngDialogData.callback();

                }, function (data, status) {
                    // Handle sign in Error
                    // Shouldn't happen, maybe remove this
                });
            }, function (data, status) {
                // Handle Registration Error
            });
        }

    }]);

})();
