/// <reference path="../../Services/RoutingService.js" />
/// <reference path="../../Services/AccountService.js" />
(function () {
    'use strict';

    angular
        .module('VoteOn-Account')
        .directive('toolbar', toolbar);

    function toolbar() {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/Scripts/Directives/Toolbar/Toolbar.html',
            scope: {},
            controller: ToolbarController
        };
    }


    ToolbarController.$inject = ['$scope', 'RoutingService', 'AccountService'];

    function ToolbarController($scope, RoutingService, AccountService) {

        $scope.isLoggedIn = false;

        $scope.login = login;
        $scope.register = register;
        $scope.logout = logout;

        activate();

        function activate() {
            AccountService.registerAccountObserver(updateAccountStatus);
            updateAccountStatus();
        }

        function login() {
            RoutingService.navigateToLoginPage();
        }

        function register() {
            RoutingService.navigateToRegisterPage();
        }

        function logout() {
            AccountService.logout();
        }

        function updateAccountStatus() {
            if (AccountService.account === undefined || AccountService.account === null) {
                $scope.isLoggedIn = false;
            }
            else {
                $scope.isLoggedIn = true;
            }
        }
    }
})();