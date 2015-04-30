/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/PollService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('RegisteredDashboardController', RegisteredDashboardController);

    RegisteredDashboardController.$inject = ['$scope', 'AccountService', 'PollService', 'RoutingService', 'TokenService'];

    function RegisteredDashboardController($scope, AccountService, PollService, RoutingService, TokenService) {

        $scope.account = AccountService.account;
        $scope.createPoll = createNewPoll;
        $scope.getUserPolls = getUserPolls;
        $scope.manageUrl = manageUrl;
        $scope.copyPoll = copyPoll;

        $scope.userPolls = {};

        activate();


        function activate() {
            AccountService.registerAccountObserver(function () {
                $scope.account = AccountService.account;
            });

            getUserPolls();
        }


        function createNewPoll(question) {
            PollService.createPoll(question, createPollSuccessCallback);
        }

        function createPollSuccessCallback(data) {
            TokenService.setToken(data.UUID, data.CreatorBallot.TokenGuid);
            navigateToManagePage(data.ManageId);
        }

        function getUserPolls() {
            PollService.getUserPolls()
                .then(function (data) {
                    $scope.userPolls = data;
                });
        }

        function navigateToManagePage(manageId) {
            return RoutingService.navigateToManagePage(manageId);
        }

        function manageUrl(manageId) {
            return RoutingService.getManagePageUrl(manageId);
        }

        function copyPoll(pollId) {
            PollService.copyPoll(pollId)
                .then(function (data) {
                    navigateToManagePage(data.newManageId);
                });
        }
    }

})();
