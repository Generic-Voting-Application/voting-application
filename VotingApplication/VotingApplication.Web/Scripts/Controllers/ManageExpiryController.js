/// <reference path="../Services/ManageService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageExpiryController', ManageExpiryController);

    ManageExpiryController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];

    function ManageExpiryController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;
        $scope.timeChoice = null;

        $scope.updatePoll = updatePoll;
        $scope.return = navigateToManagePage;
        $scope.formatExpiry = formatExpiry;
        $scope.removeExpiry = removeExpiry;
        $scope.setDate = setDate;
        $scope.timeOffset = timeOffset;
        $scope.dateFilter = dateFilter;

        activate();

        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            });
        }

        function formatExpiry() {
            if ($scope.poll && $scope.poll.ExpiryDateUtc) {
                return moment($scope.poll.ExpiryDateUtc).format('dddd, MMMM Do YYYY, HH:mm');
            }
            return 'Never';
        }

        function updatePoll() {
            ManageService.updatePollExpiry($routeParams.manageId, $scope.poll.ExpiryDateUtc)
            .then(navigateToManagePage);
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function setDate(choice) {
            var newDate = moment().add(1, choice);
            newDate.minutes(Math.ceil(newDate.minutes() / 5) * 5);
            $scope.poll.ExpiryDateUtc = newDate.toDate();
            $scope.timeChoice = choice;
        }

        function removeExpiry() {
            $scope.poll.ExpiryDateUtc = null;
            $scope.timeChoice = null;
        }

        function dateFilter(date) {
            return moment(date).isAfter(moment().startOf('Day'));
        }

        function timeOffset() {
            if ($scope.poll && $scope.poll.ExpiryDateUtc) {
                var now = moment();

                var hourAwayLower = moment(now).add(1, 'hour').subtract(2.5, 'minutes');
                var hourAwayUpper = moment(hourAwayLower).add(5, 'minutes');
                if (moment($scope.poll.ExpiryDateUtc).isBetween(hourAwayLower, hourAwayUpper)) {
                    return 'hour';
                }

                var dayAwayLower = moment(now).add(1, 'day').subtract(2.5, 'minutes');
                var dayAwayUpper = moment(dayAwayLower).add(5, 'minutes');
                if (moment($scope.poll.ExpiryDateUtc).isBetween(dayAwayLower, dayAwayUpper)) {
                    return 'day';
                }

                var weekAwayLower = moment(now).add(1, 'week').subtract(2.5, 'minutes');
                var weekAwayUpper = moment(weekAwayLower).add(5, 'minutes');
                if (moment($scope.poll.ExpiryDateUtc).isBetween(weekAwayLower, weekAwayUpper)) {
                    return 'week';
                }
            }
        }
    }

})();
