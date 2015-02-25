(function () {
    angular.module('GVA.Common').controller('AccountLoginController', ['$scope', 'AccountService', function ($scope, AccountService) {

        $scope.loginAccount = function (form) {

            AccountService.getAccessToken(form.email, form.password, function (data) {
                AccountService.setAccount(data.access_token);

                $scope.closeThisDialog();
                if ($scope.ngDialogData.callback) $scope.ngDialogData.callback();

            }, function (data, status) {
                // Handle Error
            });
        }

    }]);

})();
