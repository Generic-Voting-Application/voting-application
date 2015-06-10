/// <reference path="../Services/AccountService.js" />
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
