/// <reference path="../Services/AccountService.js" />
/// <reference path="../Services/ManageService.js" />
/// <reference path="../Services/RoutingService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManagePageController', ManagePageController);

    ManagePageController.$inject = ['$scope', '$routeParams', 'ManageService', 'RoutingService'];

    function ManagePageController($scope, $routeParams, ManageService, RoutingService) {

        var manageId = $routeParams.manageId;

        $scope.poll = {};
        $scope.invitees = [];
        $scope.manageId = manageId;
        $scope.updatePoll = updatePollDetails;
        $scope.discardNameChanges = discardNameChanges;
        $scope.formatPollExpiry = formatPollExpiryDate;
        $scope.selectText = selectTargetText;
        $scope.dateFilter = dateFilter;
        $scope.pollUrl = pollUrl;
        $scope.manageSubPageUrl = manageSubPageUrl;
        $scope.visited = false;

        activate();

        function dateFilter(date) {
            var startOfDay = new Date();
            startOfDay.setHours(0);
            return date >= startOfDay;
        }

        function activate() {
            ManageService.getPoll(manageId, function (data) {
                $scope.poll = data;
                $scope.invitees = data.Voters.filter(function (voter) {
                    return voter.Email !== null;
                });
            });
            $scope.visited = ManageService.getVisited(manageId);
            if (!$scope.visited) {
                ManageService.setVisited(manageId);
            }
        }

        function updatePollDetails() {
            ManageService.poll = $scope.poll;
            ManageService.updatePoll($routeParams.manageId, $scope.poll);
        }

        function discardNameChanges() {
            ManageService.getPoll(manageId, function (data) {
                $scope.poll = data;
            });
        }

        function formatPollExpiryDate() {
            if (!$scope.poll.ExpiryDate) {
                return 'Never';
            }

            var expiryDate = new Date($scope.poll.ExpiryDate);
            return moment(expiryDate).format('ddd, MMM Do YYYY, HH:mm');
        }

        function selectTargetText($event) {
            $event.target.select();
        }

        function pollUrl() {
            return RoutingService.getVotePageUrl($scope.poll.UUID);
        }

        function manageSubPageUrl(subPage) {
            return RoutingService.getManagePageUrl($scope.manageId, subPage);
        }
    }
})();
