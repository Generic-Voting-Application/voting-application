/// <reference path="../Services/ManageService.js" />
(function () {
    angular
        .module('GVA.Creation')
        .controller('ManageExpiryController', ManageExpiryController);

    ManageExpiryController.$inject = ['$scope', '$routeParams', '$location', 'ManageService'];

    function ManageExpiryController($scope, $routeParams, $location, ManageService) {

        $scope.poll = ManageService.poll;
        $scope.manageId = $routeParams.manageId;
        $scope.timeOption = null;

        $scope.updatePoll = updatePollDetails;
        $scope.return = navigateToManagePage;
        $scope.formatExpiry = formatExpiry;
        $scope.removeExpiry = removeExpiry;
        $scope.setDate = setDate;
        $scope.timeOffset = timeOffset;

        activate();
        
        function activate() {
            ManageService.registerPollObserver(function () {
                $scope.poll = ManageService.poll;
            });
        }

        function formatExpiry() {
            if ($scope.poll && $scope.poll.ExpiryDate){
                return moment($scope.poll.ExpiryDate).format("dddd, MMMM Do YYYY, HH:mm");
            }
            return 'Never';
        }

        function navigateToManagePage() {
            $location.path('Manage/' + $scope.manageId);
        };

        function updatePollDetails() {
            ManageService.updatePoll($routeParams.manageId, $scope.poll, updatePollSuccessCallback);
        };

        function updatePollSuccessCallback() {
            ManageService.getPoll($scope.manageId);
        };

        function setDate(option) {
            $scope.poll.ExpiryDate = moment().add(1, option);
            $scope.timeOption = option;
        }

        function removeExpiry() {
            $scope.poll.ExpiryDate = null;
            $scope.timeOption = null;
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
