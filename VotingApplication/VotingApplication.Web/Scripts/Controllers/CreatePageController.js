/// <reference path="../Services/AccountService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('CreatePageController', CreatePageController);

    CreatePageController.$inject = ['$scope', 'AccountService'];

    function CreatePageController($scope, AccountService) {

        $scope.account = AccountService.account;
        $scope.openLoginDialog = showLoginDialog;
        $scope.signOut = AccountService.clearAccount;
        $scope.isLoggedIn = false;
        $scope.$watch(
            function () { return AccountService.account },
            function () {
                $scope.isLoggedIn = (AccountService.account != null);
            });


        activate();


        function showLoginDialog() {
            AccountService.openLoginDialog($scope);
        }

        function activate() {
            AccountService.registerAccountObserver(function () {
                $scope.account = AccountService.account;
            });
        }
    };
})();
