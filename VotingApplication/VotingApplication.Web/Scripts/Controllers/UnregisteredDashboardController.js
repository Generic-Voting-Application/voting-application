/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('UnregisteredDashboardController', UnregisteredDashboardController);

    UnregisteredDashboardController.$inject = ['$scope', 'AccountService', 'PollService'];

    function UnregisteredDashboardController($scope, AccountService, PollService) {

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
            window.location.href = '/#/Manage/' + data.ManageId;
        }
    }

})();
