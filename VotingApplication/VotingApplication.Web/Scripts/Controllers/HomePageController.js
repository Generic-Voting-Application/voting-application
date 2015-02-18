(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('HomePageController', ['$scope', 'PollAction', function ($scope, PollAction) {
        $scope.models = {
            pageTitle: 'Pollster'
        };

        $scope.pollExpiry = undefined;

        var timeStringForTimeout = function (expiryDate, timeUnit, timeUnitName) {
            var secondsToExpiry = Math.floor((expiryDate - Date.now()) / 1000);

            var timeValue = Math.floor(secondsToExpiry / timeUnit);
            timeUnitName = (timeValue == 1) ? timeUnitName : timeUnitName + 's';

            var timeString = timeValue + ' ' + timeUnitName;

            //Update at the next point we expect a change
            var timeoutTime = 1000 * (secondsToExpiry - 1 - timeValue * timeUnit);
            setTimeout(calculateExpiry, timeoutTime, expiryDate);

            return timeString;
        }

        var calculateExpiry = function (expiryDate) {
            if (expiryDate < Date.now()) {
                $scope.pollExpiry = 'Poll closed';
                $scope.$apply();
            } else {
                var timeString;

                var secondsToExpiry = (expiryDate - Date.now()) / 1000;

                //NB: Months is an approximation, we don't need to be absolutely exact here.
                if (secondsToExpiry > 60 * 60 * 24 * 28) {
                    timeString = timeStringForTimeout(expiryDate, 60 * 60 * 24 * 28, 'month');
                }
                else if (secondsToExpiry > 60 * 60 * 24) {
                    timeString = timeStringForTimeout(expiryDate, 60 * 60 * 24, 'day');
                }
                else if (secondsToExpiry > 60 * 60) {
                    timeString = timeStringForTimeout(expiryDate, 60 * 60, 'hour');
                }
                else if (secondsToExpiry > 60) {
                    timeString = timeStringForTimeout(expiryDate, 60, 'minute');
                }
                else {
                    timeString = timeStringForTimeout(expiryDate, 1, 'second');
                }

                $scope.pollExpiry = 'Poll closes in: ' + timeString;
                $scope.$apply();
            }
        }

        PollAction.getPoll(PollAction.currentPollId(), function (data) {
            $scope.pollName = data.Name
            if (data.Expires) {
                calculateExpiry(new Date(data.ExpiryDate));
            }
        });
    }]);

})();