(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('AccountLoginController', ['$scope', 'AccountService', function ($scope, AccountService) {

        $scope.loginAccount = function (form) {

            AccountService.logIn(form.email, form.password, function (data) {
                AccountService.setAccount(data.access_token);

                $scope.closeThisDialog();
                if ($scope.ngDialogData.callback) $scope.ngDialogData.callback();

            }, function (data, status) {
                // Handle Error
            });
        }

    }]);

})();
