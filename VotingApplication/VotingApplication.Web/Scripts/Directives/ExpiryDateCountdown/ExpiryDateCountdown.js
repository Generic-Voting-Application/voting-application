(function () {
    'use strict';

    angular
        .module('VoteOn-Poll')
        .directive('expiryDateCountdown', expiryDateCountdown);

    expiryDateCountdown.$inject = [];

    function expiryDateCountdown() {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/Scripts/Directives/ExpiryDateCountdown/ExpiryDateCountdown.html',
            scope: {
                pollExpiredCallback: '&',
                expiryDateUtc: '='
            },
            controller: ExpiryDateCountdownController
        };
    }

    ExpiryDateCountdownController.$inject = ['$scope', '$timeout'];

    function ExpiryDateCountdownController($scope, $timeout) {

        $scope.timeToExpiry = '';

        $scope.timer = null;
        $scope.$on('$destroy', clearTimer);

        $scope.$watch('expiryDateUtc', updateTimeToExpiry);

        function updateTimeToExpiry(newExpiryDateUtc) {
            if (newExpiryDateUtc === null) {
                clearTimer();
                return;
            }

            var expiryDateUtc = moment.utc(newExpiryDateUtc);

            if (expiryDateUtc < moment.utc()) {
                pollExpired();
                return;
            }

            calculateTimeToExpiry(expiryDateUtc);
        }

        function pollExpired() {
            $scope.timeToExpiry = 'Poll closed';
            clearTimer();
            $scope.pollExpiredCallback();
        }

        function calculateTimeToExpiry(expiryDateUtc) {

            var now = moment.utc();

            if (difference(expiryDateUtc, now, 'months') > 0) {
                calculateUnit(expiryDateUtc, now, 'months');

            }
            else if (difference(expiryDateUtc, now, 'days') > 0) {
                calculateUnit(expiryDateUtc, now, 'days');

            }
            else if (difference(expiryDateUtc, now, 'hours') > 0) {
                calculateUnit(expiryDateUtc, now, 'hours');

            }
            else if (difference(expiryDateUtc, now, 'minutes') > 0) {
                calculateUnit(expiryDateUtc, now, 'minutes');

            }
            else {
                calculateUnit(expiryDateUtc, now, 'seconds');
            }

        }

        function difference(first, second, unit) {
            return first.diff(second, unit);
        }

        function calculateUnit(expiryDate, now, unit) {
            var unitValueRemaining = difference(expiryDate, now, unit);

            var exactDifference = difference(expiryDate, now, 'milliseconds');
            var differenceRoundedDownToNextUnit = moment.duration(unitValueRemaining, unit).asMilliseconds();

            var millisecondsToNextUpdate = moment.duration(exactDifference - differenceRoundedDownToNextUnit, 'milliseconds').asMilliseconds();

            updateDisplay(unitValueRemaining, unit);
            scheduleNextUpdate(expiryDate, millisecondsToNextUpdate);
        }

        function updateDisplay(unitValue, displayUnit) {
            // Strip trailing 's' from unit if it's singular.
            if (unitValue === 1) {
                displayUnit = displayUnit.slice(0, -1);
            }

            $scope.timeToExpiry = 'Poll closes in ' + unitValue + ' ' + displayUnit;
        }

        function scheduleNextUpdate(expiryDate, timeToNextUpdate) {
            clearTimer();
            $scope.timer = $timeout(function () { updateTimeToExpiry(expiryDate); }, timeToNextUpdate);
        }

        function clearTimer() {
            $timeout.cancel($scope.timer);
        }

    }
})();
