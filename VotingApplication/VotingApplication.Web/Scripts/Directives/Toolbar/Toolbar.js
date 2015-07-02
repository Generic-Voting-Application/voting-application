(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
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


    ToolbarController.$inject = ['$scope', 'RoutingService'];

    function ToolbarController($scope, RoutingService) {

        $scope.login = login;
        $scope.register = register;

        function login() {
            RoutingService.navigateToLoginPage();
        }

        function register() {
            RoutingService.navigateToRegisterPage();
        }
    }
})();