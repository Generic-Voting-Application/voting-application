/// <reference path="../Services/ManageService.js" />
(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .controller('ManageExpiryController', ManageExpiryController);

    ManageExpiryController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];

    function ManageExpiryController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.ExpiryDateLocal = null;

        $scope.manageId = $routeParams.manageId;

        $scope.updatePoll = updatePoll;
        $scope.return = navigateToManagePage;
        $scope.formatExpiry = formatExpiry;
        $scope.removeExpiry = removeExpiry;
        $scope.setDate = setDate;
        $scope.dateFilter = dateFilter;

        activate();

        function activate() {
            getManageServicePollExpiryDateAsLocal();

            ManageService.registerPollObserver(function () {
                getManageServicePollExpiryDateAsLocal();
            });
        }

        function getManageServicePollExpiryDateAsLocal() {
            var poll = ManageService.poll;
            if (poll && poll.ExpiryDateUtc) {
                $scope.ExpiryDateLocal = moment.utc(poll.ExpiryDateUtc).local();
            }
        }

        function formatExpiry() {
            if ($scope.ExpiryDateLocal) {
                return moment($scope.ExpiryDateLocal).format('dddd, MMMM Do YYYY, HH:mm');
            }
            return 'Never';
        }

        function updatePoll() {

            var ExpiryDateAsUTC;

            if ($scope.ExpiryDateLocal === null) {
                ExpiryDateAsUTC = null;
            }
            else {
                ExpiryDateAsUTC = moment($scope.ExpiryDateLocal).utc();
            }

            ManageService.updatePollExpiry($routeParams.manageId, ExpiryDateAsUTC)
            .then(navigateToManagePage);
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function setDate(choice) {
            var newDate = moment().add(1, choice);
            newDate.minutes(Math.ceil(newDate.minutes() / 5) * 5);
            $scope.ExpiryDateLocal = newDate.toDate();
        }

        function removeExpiry() {
            $scope.ExpiryDateLocal = null;
        }

        function dateFilter(date) {
            return moment(date).isAfter(moment().startOf('Day'));
        }
    }

})();
