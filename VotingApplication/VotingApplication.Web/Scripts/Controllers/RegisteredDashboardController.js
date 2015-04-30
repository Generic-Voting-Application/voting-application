/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/PollService.js" />
/// <reference path="../Services/RoutingService.js" />
/// <reference path="../Services/TokenService.js" />
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
            PollService.createPoll(question)
            .then(createPollSuccessCallback);
        }

        function createPollSuccessCallback(response) {
            var data = response.data;
            TokenService.setToken(data.UUID, data.CreatorBallot.TokenGuid)
                .then(function () { goToManagePage(data.ManageId); });
        }

        function getUserPolls() {
            PollService.getUserPolls()
                .then(function (response) {
                    $scope.userPolls = response.data;
                });
        }

        function goToManagePage(manageId) {
            return RoutingService.navigateToManagePage(manageId);
        }

        function manageUrl(manageId) {
            return RoutingService.getManagePageUrl(manageId);
        }

        function copyPoll(pollId) {
            PollService.copyPoll(pollId)
                .then(function (data) {
                    goToManagePage(data.newManageId);
                });
        }
    }

})();
