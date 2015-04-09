/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('UnregisteredDashboardController', UnregisteredDashboardController);

    UnregisteredDashboardController.$inject = ['$scope', 'AccountService', 'RoutingService', 'TokenService', 'PollService'];

    function UnregisteredDashboardController($scope, AccountService, RoutingService, TokenService, PollService) {

        $scope.openLoginDialog = showLoginDialog;
        $scope.openRegisterDialog = showRegisterDialog;
        $scope.createPoll = createNewPoll;


        function showLoginDialog() {
            AccountService.openLoginDialog($scope);
        }

        function showRegisterDialog() {
            AccountService.openRegisterDialog($scope);
        }

        function createNewPoll(question) {
            PollService.createPoll(question, createPollSuccessCallback);
        }

        function createPollSuccessCallback(data) {
            TokenService.setToken(data.UUID, data.CreatorBallot.TokenGuid);
            RoutingService.navigateToManagePage(data.manageId);
            
        }
    }

})();
