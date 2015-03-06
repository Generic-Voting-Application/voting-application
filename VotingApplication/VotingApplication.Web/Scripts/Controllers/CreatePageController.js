(function () {
    angular
        .module('GVA.Creation')
        .controller('CreatePageController', ['$scope', 'AccountService',
        function ($scope, AccountService) {

            $scope.account = AccountService.account;

            $scope.openLoginDialog = function () {
                AccountService.openLoginDialog($scope);
            }

            $scope.signOut = function () {
                AccountService.clearAccount();
            }

            AccountService.registerAccountObserver(function () {
                $scope.account = AccountService.account;
            });

        }]);
})();
