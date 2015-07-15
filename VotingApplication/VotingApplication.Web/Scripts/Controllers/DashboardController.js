/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Manage')
        .controller('DashboardController', DashboardController);

    DashboardController.$inject = ['$scope', 'AccountService', 'RoutingService'];

    function DashboardController($scope, AccountService, RoutingService) {

        $scope.account = AccountService.account;
        $scope.openLoginDialog = showLoginDialog;
        $scope.openRegisterDialog = openRegisterDialog;
        $scope.signOut = AccountService.clearAccount;
        $scope.myPollsLink = RoutingService.getMyPollsUrl();
        
        activate();

        function showLoginDialog() {
            RoutingService.navigateToLoginPage();
        }

        function openRegisterDialog() {
            RoutingService.navigateToRegisterPage();
        }

        function activate() {
            AccountService.registerAccountObserver(function () {
                $scope.account = AccountService.account;
            });
        }
    }
})();
