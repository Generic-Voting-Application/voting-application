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

        var UNAUTHORISED = 401;

        $scope.createPoll = createNewPoll;
        $scope.getUserPolls = getUserPolls;
        $scope.manageUrl = manageUrl;
        $scope.copyPoll = copyPoll;

        $scope.userPolls = {};

        activate();


        function activate() {
            getUserPolls();
        }


        function createNewPoll(question) {
            PollService.createPoll(question)
            .then(createPollSuccessCallback);
        }

        function createPollSuccessCallback(response) {
            var data = response.data;

            TokenService.setManageId(data.UUID, data.ManageId)
               .then(TokenService.setToken(data.UUID, data.CreatorBallot.TokenGuid)
               .then(function () {
                   RoutingService.navigateToManagePage(data.ManageId);
               }));
        }

        function getUserPolls() {
            PollService.getUserPolls()
                .then(function (response) {
                    $scope.userPolls = response.data;
                })
                .catch(function (response) {
                    if (response.status === UNAUTHORISED) {
                        AccountService.clearAccount();
                    }
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
                .then(saveTokenAndGoToManage);
        }

        function saveTokenAndGoToManage(response) {
            var data = response.data;

            TokenService.setToken(data.NewPollId, data.CreatorBallotToken)
                .then(function () { goToManagePage(data.NewManageId); });

        }
    }

})();
