/// <reference path="../Services/AccountService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('DashboardController', DashboardController);

    DashboardController.$inject = ['$scope', 'AccountService'];

    function DashboardController($scope, AccountService) {

        $scope.account = AccountService.account;
        $scope.openLoginDialog = showLoginDialog;
        $scope.openRegisterDialog = openRegisterDialog;
        $scope.signOut = AccountService.clearAccount;
        
        activate();

        function showLoginDialog() {
            AccountService.openLoginDialog($scope);
        }

        function openRegisterDialog() {
            AccountService.openRegisterDialog($scope);
        }

        function activate() {
            AccountService.registerAccountObserver(function () {
                $scope.account = AccountService.account;
            });
        }
    }
})();
