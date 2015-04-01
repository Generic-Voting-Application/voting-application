/// <reference path="../Services/ManageService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageExpiryController', ManageExpiryController);

    ManageExpiryController.$inject = ['$scope', '$routeParams', '$location', 'ManageService', 'RoutingService'];

    function ManageExpiryController($scope, $routeParams, $location, ManageService, RoutingService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;
        $scope.timeOption = null;

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
            if ($scope.poll && $scope.poll.ExpiryDate){
                return moment($scope.poll.ExpiryDate).format('dddd, MMMM Do YYYY, HH:mm');
            }
            return 'Never';
        }

        function updatePoll() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, navigateToManagePage);
        }

        function navigateToManagePage() {
            RoutingService.navigateToManagePage($scope.manageId);
        }

        function setDate(option) {
            var newDate = moment().add(1, option);
            newDate.minutes(Math.ceil(newDate.minutes() / 5) * 5);
            $scope.poll.ExpiryDate = newDate.toDate();
            $scope.timeOption = option;
        }

        function removeExpiry() {
            $scope.poll.ExpiryDate = null;
            $scope.timeOption = null;
        }

        function dateFilter(date) {
            return moment(date).isAfter(moment().startOf('Day'));
        }

        function timeOffset() {
            if ($scope.poll && $scope.poll.ExpiryDate){
                var now = moment();

                var hourAwayLower = moment(now).add(1, 'hour').subtract(2.5, 'minutes');
                var hourAwayUpper = moment(hourAwayLower).add(5, 'minutes');
                if (moment($scope.poll.ExpiryDate).isBetween(hourAwayLower, hourAwayUpper)) {
                    return 'hour';
                }

                var dayAwayLower = moment(now).add(1, 'day').subtract(2.5, 'minutes');
                var dayAwayUpper = moment(dayAwayLower).add(5, 'minutes');
                if (moment($scope.poll.ExpiryDate).isBetween(dayAwayLower, dayAwayUpper)) {
                    return 'day';
                }

                var weekAwayLower = moment(now).add(1, 'week').subtract(2.5, 'minutes');
                var weekAwayUpper = moment(weekAwayLower).add(5, 'minutes');
                if (moment($scope.poll.ExpiryDate).isBetween(weekAwayLower, weekAwayUpper)) {
                    return 'week';
                }
            }
        }
    }

})();
